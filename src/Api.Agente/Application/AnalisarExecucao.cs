using Api.Agente.Domain;

namespace Api.Agente.Application;

public sealed class AnalisarExecucao(ValidadorEntrada validador, ClassificadorDeItem classificador, IRepositorioExecucao repositorio)
{
    public async Task<Execucao> ExecutarAsync(PedidoAnalise pedido, CancellationToken ct)
    {
        validador.Validar(pedido);
        var execucao = new Execucao
        {
            Usuario = pedido.Usuario.Trim(),
            Empresa = pedido.Empresa.Trim(),
            Papel = pedido.Papel.Trim(),
            Finalidade = pedido.Finalidade.Trim(),
            Uf = pedido.Uf.Trim(),
            RegimeTributario = pedido.RegimeTributario.Trim(),
            TipoOperacao = pedido.TipoOperacao.Trim(),
            DataVigencia = pedido.DataVigencia,
            Intencao = pedido.Intencao
        };
        execucao.ValidacoesNaoRealizadas.Add("layout de item não registrado");

        var itens = Itens(pedido.Entrada);
        for (var i = 0; i < itens.Count; i++)
        {
            var classificado = await classificador.ClassificarAsync((i + 1).ToString(), itens[i].Descricao!.Trim(), pedido.DataVigencia, ct);
            if (pedido.Intencao == IntencaoAnalise.AuditarDeclarado)
            {
                ComparadorDeclaracao.Aplicar(classificado, itens[i].NcmDeclarada, itens[i].TributosDeclarados);
            }

            execucao.Itens.Add(classificado);
        }

        execucao.EstadoNota = CalculadoraEstadoNota.Calcular(execucao.Itens);
        foreach (var evidencia in execucao.Itens.SelectMany(i => i.Evidencias))
        {
            var versao = evidencia.Vigencia.ToString("yyyy-MM-dd");
            if (execucao.VersoesBases.All(v => v.Base != evidencia.Fonte || v.Versao != versao))
            {
                execucao.VersoesBases.Add(new VersaoBase { Base = evidencia.Fonte, Versao = versao });
            }
        }

        await repositorio.SalvarAsync(execucao, ct);
        return execucao;
    }

    public Task<Execucao?> ObterAsync(Guid execucaoId, CancellationToken ct) => repositorio.ObterAsync(execucaoId, ct);

    private static List<ItemPedido> Itens(EntradaPedido entrada)
    {
        var itens = entrada.Itens.Where(i => !string.IsNullOrWhiteSpace(i.Descricao)).ToList();
        if (itens.Count > 0)
        {
            return itens;
        }

        return [new ItemPedido { Descricao = entrada.Texto!.Trim() }];
    }
}

using Api.Agente.Domain;

namespace Api.Agente.Application;

public sealed class AnalisarExecucao(
    ValidadorEntrada validador,
    ClassificadorDeItem classificador,
    IRepositorioExecucao repositorio,
    RegistroDeLayouts layouts)
{
    public async Task<Execucao> ExecutarAsync(PedidoAnalise pedido, CancellationToken ct, bool textoLivre = false)
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

        // NFe e a ponte da SOFIA trazem a descricao livre do produto, nao o formato chave:valor.
        var descricaoLivre = textoLivre || pedido.Entrada.Tipo != TipoEntrada.Texto;
        var itens = Itens(pedido.Entrada);
        for (var i = 0; i < itens.Count; i++)
        {
            var contexto = ContextoDoItem.Resolver(pedido, itens[i]);
            var classificado = await classificador.ClassificarAsync(
                (i + 1).ToString(), itens[i].Descricao!.Trim(), contexto.DataVigencia, ct, exigirDadosEssenciais: !descricaoLivre);
            classificado.Contexto = contexto;
            if (pedido.Intencao == IntencaoAnalise.AuditarDeclarado)
            {
                ComparadorDeclaracao.Aplicar(classificado, itens[i].NcmDeclarada, ContextoDoItem.TributosDeclarados(itens[i]));
            }

            MontadorEnvelope.Aplicar(classificado, pedido.Intencao, layouts);
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
        // A NFe ja foi validada (base64, limite e layout) antes de chegar aqui.
        if (entrada.Tipo == TipoEntrada.XmlNfe)
        {
            return LeitorNfe.Ler(Convert.FromBase64String(entrada.Arquivo!.ConteudoBase64!));
        }

        var itens = entrada.Itens.Where(i => !string.IsNullOrWhiteSpace(i.Descricao)).ToList();
        if (itens.Count > 0)
        {
            return itens;
        }

        return [new ItemPedido { Descricao = entrada.Texto!.Trim() }];
    }
}

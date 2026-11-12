using Api.Agente.Domain;
using Api.Agente.Infrastructure;
using Microsoft.Extensions.Options;

namespace Api.Agente.Application;

public sealed class AtendenteSofia(
    ValidadorSolicitacaoSofia validador,
    MemoriaSolicitacaoSofia memoria,
    AnalisarExecucao analise,
    IOptions<OpcoesSofia> opcoes)
{
    public const string AnaliseSemAchado =
        "Nenhum achado foi produzido. O texto dos documentos foi tratado como dado e não como instrução.";

    public async Task<(string TarefaId, ResultadoAnaliseSofia Resultado)> AtenderAsync(
        SolicitacaoAnaliseSofia? pedido,
        CancellationToken ct)
    {
        if (pedido is null)
        {
            throw new SolicitacaoInvalidaException("Solicitação ausente.");
        }

        if (validador.Validar(pedido, opcoes.Value) is string mensagem)
        {
            throw new SolicitacaoInvalidaException(mensagem);
        }

        var id = Guid.Parse(pedido.IdSolicitacao ?? throw new SolicitacaoInvalidaException("idSolicitacao deve ser UUID."));
        if (memoria.Tentar(id, out var pronta))
        {
            return pronta;
        }

        var resultado = await CriarAsync(pedido, ct);
        return memoria.Guardar(id, resultado);
    }

    private async Task<ResultadoAnaliseSofia> CriarAsync(SolicitacaoAnaliseSofia pedido, CancellationToken ct)
    {
        var agente = Identidade();
        var analisados = new List<DocumentoAnalisadoSofia>();
        foreach (var documento in pedido.Documentos!)
        {
            analisados.Add(await AnalisarDocumentoAsync(pedido, documento, ct));
        }

        return MontadorResultadoSofia.DaAnalise(pedido.IdExecucao!, pedido.IdSolicitacao!, agente, analisados);
    }

    private async Task<DocumentoAnalisadoSofia> AnalisarDocumentoAsync(
        SolicitacaoAnaliseSofia pedido,
        DocumentoSofia documento,
        CancellationToken ct)
    {
        // A SOFIA envia a descrição textual do documento, não um JSON de pedido.
        // Classificamos essa descrição direto, com a intenção de sugerir NCM. O
        // contexto de negócio é opcional: usa o que a SOFIA informar e defaults no resto.
        var entrada = MontarPedido(pedido, documento);
        try
        {
            var execucao = await analise.ExecutarAsync(entrada, ct, textoLivre: true);
            var falha = execucao.EstadoNota == EstadoNota.FalhaTecnica;
            return new DocumentoAnalisadoSofia(
                documento.IdDocumento!.Trim(),
                execucao,
                falha ? "falha-tecnica" : null,
                falha ? execucao.Itens.SelectMany(i => i.Achados).Select(a => a.Descricao).FirstOrDefault() ?? "Falha técnica." : null);
        }
        catch (EntradaRejeitadaException ex)
        {
            return new DocumentoAnalisadoSofia(documento.IdDocumento!.Trim(), null, "pedido-recusado", ex.Message);
        }
    }

    private static PedidoAnalise MontarPedido(SolicitacaoAnaliseSofia pedido, DocumentoSofia documento)
    {
        var contexto = pedido.Contexto;
        return new PedidoAnalise
        {
            Usuario = pedido.Usuario!.Nome!.Trim(),
            Empresa = Preenchido(contexto?.Empresa, "não informado"),
            Uf = Preenchido(contexto?.Uf, "NA"),
            RegimeTributario = Preenchido(contexto?.RegimeTributario, "naoInformado"),
            TipoOperacao = Preenchido(contexto?.TipoOperacao, "naoInformado"),
            Papel = Preenchido(contexto?.Papel, "fiscal"),
            Finalidade = Preenchido(contexto?.Finalidade, "sugerir"),
            DataVigencia = contexto?.DataVigencia ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Intencao = IntencaoAnalise.SugerirNcm,
            Entrada = new EntradaPedido
            {
                Tipo = TipoEntrada.Texto,
                Texto = documento.Texto!.Trim()
            }
        };
    }

    private static string Preenchido(string? valor, string padrao) =>
        string.IsNullOrWhiteSpace(valor) ? padrao : valor.Trim();

    private AgenteSofia Identidade()
    {
        var identidade = opcoes.Value;
        return new AgenteSofia
        {
            IdAgente = string.IsNullOrWhiteSpace(identidade.IdAgente) ? "agente-fiscal" : identidade.IdAgente.Trim(),
            Nome = string.IsNullOrWhiteSpace(identidade.Nome) ? "Agente Fiscal INFIS" : identidade.Nome.Trim()
        };
    }
}

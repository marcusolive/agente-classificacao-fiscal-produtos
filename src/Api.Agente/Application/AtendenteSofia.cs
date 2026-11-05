using System.Text.Json;
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

    private static readonly (string Campo, Func<ContextoSofia?, bool> Ausente)[] Campos =
    [
        ("empresa", c => string.IsNullOrWhiteSpace(c?.Empresa)),
        ("uf", c => string.IsNullOrWhiteSpace(c?.Uf)),
        ("regimeTributario", c => string.IsNullOrWhiteSpace(c?.RegimeTributario)),
        ("dataVigencia", c => c?.DataVigencia is null),
        ("papel", c => string.IsNullOrWhiteSpace(c?.Papel)),
        ("finalidade", c => string.IsNullOrWhiteSpace(c?.Finalidade)),
        ("tipoOperacao", c => string.IsNullOrWhiteSpace(c?.TipoOperacao))
    ];

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
        var documentos = pedido.Documentos!.Select(d => d.IdDocumento!.Trim()).ToList();
        var faltas = Campos.Where(c => c.Ausente(pedido.Contexto)).Select(c => c.Campo).ToList();
        if (faltas.Count > 0)
        {
            return MontadorResultadoSofia.ContextoAusente(pedido.IdExecucao!, pedido.IdSolicitacao!, agente, documentos, faltas);
        }

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
        PedidoAnalise? entrada;
        try
        {
            entrada = JsonSerializer.Deserialize<PedidoAnalise>(documento.Texto ?? "", JsonDaExecucao.Opcoes);
        }
        catch (JsonException)
        {
            return new DocumentoAnalisadoSofia(documento.IdDocumento!.Trim(), null, "pedido-ilegivel", "O texto do documento não é o pedido de análise.");
        }

        if (entrada is null)
        {
            return new DocumentoAnalisadoSofia(documento.IdDocumento!.Trim(), null, "pedido-ilegivel", "O texto do documento não é o pedido de análise.");
        }

        AplicarContexto(entrada, pedido);
        try
        {
            var execucao = await analise.ExecutarAsync(entrada, ct);
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

    private static void AplicarContexto(PedidoAnalise entrada, SolicitacaoAnaliseSofia pedido)
    {
        var contexto = pedido.Contexto!;
        entrada.Usuario = pedido.Usuario!.Nome!.Trim();
        entrada.Empresa = contexto.Empresa!.Trim();
        entrada.Uf = contexto.Uf!.Trim();
        entrada.RegimeTributario = contexto.RegimeTributario!.Trim();
        entrada.DataVigencia = contexto.DataVigencia!.Value;
        entrada.Papel = contexto.Papel!.Trim();
        entrada.Finalidade = contexto.Finalidade!.Trim();
        entrada.TipoOperacao = contexto.TipoOperacao!.Trim();
        entrada.Intencao = IntencaoAnalise.SugerirNcm;
    }

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

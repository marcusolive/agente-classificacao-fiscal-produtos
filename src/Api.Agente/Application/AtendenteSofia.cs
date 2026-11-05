using Api.Agente.Domain;
using Api.Agente.Infrastructure;
using Microsoft.Extensions.Options;

namespace Api.Agente.Application;

public sealed class AtendenteSofia(
    ValidadorSolicitacaoSofia validador,
    MemoriaSolicitacaoSofia memoria,
    IOptions<OpcoesSofia> opcoes)
{
    public const string AnaliseSemAchado =
        "Nenhum achado foi produzido. O texto dos documentos foi tratado como dado e não como instrução.";

    public (string TarefaId, ResultadoAnaliseSofia Resultado) Atender(SolicitacaoAnaliseSofia? pedido)
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
        return memoria.ObterOuCriar(id, () => Criar(pedido));
    }

    private ResultadoAnaliseSofia Criar(SolicitacaoAnaliseSofia pedido)
    {
        var identidade = opcoes.Value;
        return new ResultadoAnaliseSofia
        {
            IdExecucao = pedido.IdExecucao!,
            IdSolicitacao = pedido.IdSolicitacao!,
            Agente = new AgenteSofia
            {
                IdAgente = Texto(identidade.IdAgente, "agente-fiscal"),
                Nome = Texto(identidade.Nome, "Agente Fiscal INFIS")
            },
            Status = "concluido",
            Analise = AnaliseSemAchado,
            Observacoes = pedido.Documentos!.Select(d => d.IdDocumento!.Trim()).ToList()
        };
    }

    private static string Texto(string valor, string padrao) =>
        string.IsNullOrWhiteSpace(valor) ? padrao : valor.Trim();
}

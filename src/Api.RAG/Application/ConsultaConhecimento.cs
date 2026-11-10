using Api.RAG.Domain;
using Microsoft.Extensions.Options;

namespace Api.RAG.Application;

public sealed class ConsultaConhecimento(
    IRepositorioDocumento repositorio,
    IProvedorEmbedding embedding,
    IOptions<OpcoesEmbedding> opcoes)
{
    public async Task<IReadOnlyList<EvidenciaConsultada>> ConsultarAsync(string? texto, DateOnly vigencia, CancellationToken ct)
    {
        if (!IngestaoDocumento.BuscaConfigurada(opcoes.Value))
        {
            throw new BuscaIndisponivelException("A busca vetorial não está configurada.");
        }

        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new BuscaIndisponivelException("texto é obrigatório.");
        }

        var consulta = await embedding.IncorporarConsultaAsync(texto, ct);
        if (consulta.Length != opcoes.Value.Dimensao)
        {
            throw new BuscaIndisponivelException("A dimensão do embedding não confere.");
        }

        return await repositorio.BuscarAsync(consulta, vigencia, opcoes.Value.TopK!.Value, opcoes.Value.Limiar!.Value, ct);
    }
}

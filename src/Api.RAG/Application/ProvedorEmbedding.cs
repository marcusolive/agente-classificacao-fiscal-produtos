using Api.RAG.Domain;

namespace Api.RAG.Application;

public interface IProvedorEmbedding
{
    Task IncorporarAsync(Documento documento, CancellationToken ct);

    Task<float[]> IncorporarConsultaAsync(string texto, CancellationToken ct);
}

public sealed class BuscaIndisponivelException(string message) : Exception(message);

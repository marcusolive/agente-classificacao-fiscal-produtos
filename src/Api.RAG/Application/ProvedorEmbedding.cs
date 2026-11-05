using Api.RAG.Domain;

namespace Api.RAG.Application;

public interface IProvedorEmbedding
{
    Task IncorporarAsync(Documento documento, CancellationToken ct);
}

public sealed class ProvedorEmbeddingIndisponivel : IProvedorEmbedding
{
    public Task IncorporarAsync(Documento documento, CancellationToken ct) =>
        Task.FromException(new BuscaIndisponivelException("O provedor de embeddings não está configurado."));
}

public sealed class BuscaIndisponivelException(string message) : Exception(message);

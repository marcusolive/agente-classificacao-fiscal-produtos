using Api.RAG.Domain;

namespace Api.RAG.Application;

public interface IRepositorioDocumento
{
    Task<Documento?> ObterAsync(Guid documentoId, CancellationToken ct);
    Task<Documento?> ObterPorIdentidadeAsync(string origem, string identificacao, string versao, CancellationToken ct);
    Task<IReadOnlyList<Documento>> ListarAsync(string origem, string identificacao, CancellationToken ct);
    Task<IReadOnlyList<Documento>> ListarTodosAsync(CancellationToken ct);
    Task<IReadOnlyList<EvidenciaConsultada>> BuscarAsync(
        float[] consulta,
        DateOnly vigencia,
        int topK,
        double limiar,
        CancellationToken ct);
    Task SalvarAsync(Documento documento, CancellationToken ct);
}

using System.Collections.Concurrent;
using System.Text.Json;
using Api.RAG.Application;
using Api.RAG.Domain;

namespace Api.RAG.Infrastructure;

public sealed class RepositorioDocumentoMemoria : IRepositorioDocumento
{
    private readonly ConcurrentDictionary<Guid, string> _porId = new();
    private readonly ConcurrentDictionary<string, Guid> _porIdentidade = new();

    public Task<Documento?> ObterAsync(Guid documentoId, CancellationToken ct)
    {
        if (!_porId.TryGetValue(documentoId, out var json))
        {
            return Task.FromResult<Documento?>(null);
        }

        return Task.FromResult(JsonSerializer.Deserialize<Documento>(json, JsonDaBase.Opcoes));
    }

    public Task<Documento?> ObterPorIdentidadeAsync(string origem, string identificacao, string versao, CancellationToken ct)
    {
        if (!_porIdentidade.TryGetValue(Chave(origem, identificacao, versao), out var id))
        {
            return Task.FromResult<Documento?>(null);
        }

        return ObterAsync(id, ct);
    }

    public Task<IReadOnlyList<Documento>> ListarAsync(string origem, string identificacao, CancellationToken ct)
    {
        IReadOnlyList<Documento> lista = _porId.Values
            .Select(json => JsonSerializer.Deserialize<Documento>(json, JsonDaBase.Opcoes))
            .Where(d => d is not null && d.Origem == origem && d.Identificacao == identificacao)
            .Cast<Documento>()
            .OrderBy(d => d.Versao, StringComparer.Ordinal)
            .ToList();
        return Task.FromResult(lista);
    }

    public Task SalvarAsync(Documento documento, CancellationToken ct)
    {
        _porId[documento.DocumentoId] = JsonSerializer.Serialize(documento, JsonDaBase.Opcoes);
        _porIdentidade[Chave(documento.Origem, documento.Identificacao, documento.Versao)] = documento.DocumentoId;
        return Task.CompletedTask;
    }

    private static string Chave(string origem, string identificacao, string versao) =>
        $"{origem}\u001f{identificacao}\u001f{versao}";
}

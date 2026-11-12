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

    public Task<IReadOnlyList<Documento>> ListarTodosAsync(CancellationToken ct)
    {
        IReadOnlyList<Documento> lista = _porId.Values
            .Select(json => JsonSerializer.Deserialize<Documento>(json, JsonDaBase.Opcoes))
            .Where(d => d is not null)
            .Cast<Documento>()
            .ToList();
        return Task.FromResult(lista);
    }

    public async Task<IReadOnlyList<EvidenciaConsultada>> BuscarAsync(
        float[] consulta,
        DateOnly vigencia,
        int topK,
        double limiar,
        CancellationToken ct)
    {
        var candidatos = new List<Candidato>();
        foreach (var documento in await ListarTodosAsync(ct))
        {
            if (!Serve(documento, vigencia))
            {
                continue;
            }

            foreach (var chunk in documento.Chunks)
            {
                if (chunk.Embedding is not { Length: > 0 } vetor)
                {
                    continue;
                }

                var similaridade = SimilaridadeCosseno.Calcular(consulta, vetor);
                if (similaridade < limiar)
                {
                    continue;
                }

                candidatos.Add(new Candidato(
                    new EvidenciaConsultada
                    {
                        Fonte = documento.Fonte,
                        FaixaAutoridade = documento.Faixa,
                        Trecho = chunk.Texto,
                        Localizacao = chunk.Localizacao,
                        Vigencia = vigencia,
                        Ncm = documento.Ncm,
                        Tributos = documento.Tributos
                    },
                    similaridade,
                    chunk.Posicao));
            }
        }

        return candidatos
            .OrderByDescending(c => c.Similaridade)
            .ThenBy(c => c.Evidencia.Fonte, StringComparer.Ordinal)
            .ThenBy(c => c.Posicao)
            .Take(topK)
            .Select(c => c.Evidencia)
            .ToList();
    }

    public Task SalvarAsync(Documento documento, CancellationToken ct)
    {
        _porId[documento.DocumentoId] = JsonSerializer.Serialize(documento, JsonDaBase.Opcoes);
        _porIdentidade[Chave(documento.Origem, documento.Identificacao, documento.Versao)] = documento.DocumentoId;
        return Task.CompletedTask;
    }

    public Task<int> RemoverPorOrigemAsync(string origem, CancellationToken ct)
    {
        var removidos = 0;
        foreach (var par in _porId.ToArray())
        {
            var documento = JsonSerializer.Deserialize<Documento>(par.Value, JsonDaBase.Opcoes);
            if (documento is null || documento.Origem != origem)
            {
                continue;
            }

            if (_porId.TryRemove(par.Key, out _))
            {
                removidos++;
            }

            _porIdentidade.TryRemove(Chave(documento.Origem, documento.Identificacao, documento.Versao), out _);
        }

        return Task.FromResult(removidos);
    }

    private static bool Serve(Documento documento, DateOnly vigencia) =>
        documento.Consultavel
        && documento.Estado == EstadoDocumento.Gravado
        && documento.Faixa != FaixaAutoridade.CasoCorporativoConfirmado
        && documento.VigenciaInicial <= vigencia
        && (documento.VigenciaFinal is null || documento.VigenciaFinal >= vigencia);

    private static string Chave(string origem, string identificacao, string versao) =>
        $"{origem}\u001f{identificacao}\u001f{versao}";

    private sealed record Candidato(EvidenciaConsultada Evidencia, double Similaridade, int Posicao);
}

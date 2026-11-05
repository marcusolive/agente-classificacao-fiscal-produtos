using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Api.RAG.Domain;
using Microsoft.Extensions.Options;

namespace Api.RAG.Application;

public sealed class ProvedorEmbeddingOpenAi(
    IHttpClientFactory clientes,
    IOptions<OpcoesOpenAi> openAi,
    IOptions<OpcoesEmbedding> embedding) : IProvedorEmbedding
{
    public const string Cliente = "openai-embeddings";
    private const string EndpointPadrao = "https://api.openai.com/v1";

    public async Task IncorporarAsync(Documento documento, CancellationToken ct)
    {
        if (documento.Chunks.Count == 0)
        {
            return;
        }

        var vetores = await GerarAsync(documento.Chunks.Select(chunk => chunk.Texto).ToList(), ct);
        for (var i = 0; i < documento.Chunks.Count; i++)
        {
            documento.Chunks[i].Embedding = vetores[i];
        }
    }

    public Task<float[]> IncorporarConsultaAsync(string texto, CancellationToken ct) =>
        GerarUmAsync(texto, ct);

    private async Task<float[]> GerarUmAsync(string texto, CancellationToken ct)
    {
        var vetores = await GerarAsync([texto], ct);
        return vetores[0];
    }

    private async Task<float[][]> GerarAsync(IReadOnlyList<string> textos, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(openAi.Value.ApiKey) || string.IsNullOrWhiteSpace(embedding.Value.Model))
        {
            throw new BuscaIndisponivelException("O provedor de embeddings não está configurado.");
        }

        var extras = Math.Max(0, embedding.Value.Tentativas);
        for (var tentativa = 0; ; tentativa++)
        {
            try
            {
                return await ChamarAsync(textos, ct);
            }
            catch (HttpRequestException) when (tentativa < extras)
            {
            }
            catch (TimeoutException) when (tentativa < extras)
            {
            }
        }
    }

    private async Task<float[][]> ChamarAsync(IReadOnlyList<string> textos, CancellationToken ct)
    {
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limite.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, embedding.Value.TimeoutSegundos)));
        var pedido = new HttpRequestMessage(HttpMethod.Post, $"{Base()}/embeddings")
        {
            Content = JsonContent.Create(new { model = embedding.Value.Model, input = textos })
        };
        pedido.Headers.Authorization = new AuthenticationHeaderValue("Bearer", openAi.Value.ApiKey);
        var cliente = clientes.CreateClient(Cliente);

        HttpResponseMessage resposta;
        try
        {
            resposta = await cliente.SendAsync(pedido, limite.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("O provedor de embeddings não respondeu.");
        }

        using (resposta)
        {
            if (resposta.StatusCode is System.Net.HttpStatusCode.RequestTimeout or System.Net.HttpStatusCode.TooManyRequests
                || (int)resposta.StatusCode >= 500)
            {
                throw new HttpRequestException("O provedor de embeddings não respondeu.");
            }

            if (!resposta.IsSuccessStatusCode)
            {
                throw new BuscaIndisponivelException("O provedor de embeddings recusou a chamada.");
            }

            var corpo = await resposta.Content.ReadFromJsonAsync<RespostaEmbedding>(JsonSerializerOptions.Web, limite.Token)
                ?? throw new BuscaIndisponivelException("O provedor de embeddings recusou a chamada.");
            if (corpo.Data.Count != textos.Count)
            {
                throw new BuscaIndisponivelException("O provedor de embeddings recusou a chamada.");
            }

            var dimensao = embedding.Value.Dimensao;
            var vetores = new float[textos.Count][];
            foreach (var item in corpo.Data)
            {
                if (item.Index < 0 || item.Index >= textos.Count || item.Embedding.Length != dimensao)
                {
                    throw new BuscaIndisponivelException("A dimensão do embedding não confere.");
                }

                vetores[item.Index] = item.Embedding;
            }

            if (vetores.Any(vetor => vetor is null))
            {
                throw new BuscaIndisponivelException("O provedor de embeddings recusou a chamada.");
            }

            return vetores!;
        }
    }

    private string Base()
    {
        var endpoint = openAi.Value.Endpoint.Trim().TrimEnd('/');
        return string.IsNullOrWhiteSpace(endpoint) ? EndpointPadrao : endpoint;
    }

    private sealed class RespostaEmbedding
    {
        public List<ItemEmbedding> Data { get; set; } = [];
    }

    private sealed class ItemEmbedding
    {
        public int Index { get; set; }
        public float[] Embedding { get; set; } = [];
    }
}

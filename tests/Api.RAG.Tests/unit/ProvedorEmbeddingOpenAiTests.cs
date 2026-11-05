using System.Net;
using System.Text;
using Api.RAG.Application;
using Api.RAG.Domain;
using Microsoft.Extensions.Options;

namespace Api.RAG.Tests.unit;

public class ProvedorEmbeddingOpenAiTests
{
    [Fact]
    public async Task Sem_chave_nao_chama_o_provedor()
    {
        var chamadas = 0;
        var provedor = Criar(new OpcoesOpenAi(), _ =>
        {
            chamadas++;
            return new HttpResponseMessage(HttpStatusCode.OK);
        }, tentativas: 0);

        var erro = await Assert.ThrowsAsync<BuscaIndisponivelException>(() =>
            provedor.IncorporarConsultaAsync("texto", CancellationToken.None));

        Assert.Equal("O provedor de embeddings não está configurado.", erro.Message);
        Assert.Equal(0, chamadas);
    }

    [Fact]
    public async Task Resposta_respeita_o_indice_e_a_dimensao()
    {
        var provedor = Criar(new OpcoesOpenAi { ApiKey = "teste", Endpoint = "http://embeddings.local" }, _ =>
            Json(HttpStatusCode.OK, """{"data":[{"index":1,"embedding":[0,1]},{"index":0,"embedding":[1,0]}]}"""), 0);
        var documento = new Documento
        {
            Origem = "oficial",
            Identificacao = "ncm",
            Versao = "1",
            Hash = "abc",
            Chunks =
            [
                new Chunk { Texto = "a", HashDocumento = "abc" },
                new Chunk { Texto = "b", HashDocumento = "abc" }
            ]
        };

        await provedor.IncorporarAsync(documento, CancellationToken.None);

        var primeiro = documento.Chunks[0].Embedding;
        var segundo = documento.Chunks[1].Embedding;
        Assert.NotNull(primeiro);
        Assert.NotNull(segundo);
        Assert.Equal([1f, 0f], primeiro);
        Assert.Equal([0f, 1f], segundo);
    }

    [Fact]
    public async Task Falha_transitória_repete_e_recusa_permanente_nao()
    {
        var chamadas = 0;
        var provedor = Criar(new OpcoesOpenAi { ApiKey = "teste" }, _ =>
        {
            chamadas++;
            return chamadas == 1
                ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                : Json(HttpStatusCode.OK, """{"data":[{"index":0,"embedding":[1,0]}]}""");
        }, tentativas: 1);

        var vetor = await provedor.IncorporarConsultaAsync("texto", CancellationToken.None);
        Assert.Equal([1f, 0f], vetor);
        Assert.Equal(2, chamadas);

        chamadas = 0;
        var recusa = Criar(new OpcoesOpenAi { ApiKey = "teste" }, _ =>
        {
            chamadas++;
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }, tentativas: 2);
        await Assert.ThrowsAsync<BuscaIndisponivelException>(() =>
            recusa.IncorporarConsultaAsync("texto", CancellationToken.None));
        Assert.Equal(1, chamadas);
    }

    private static ProvedorEmbeddingOpenAi Criar(
        OpcoesOpenAi openAi,
        Func<HttpRequestMessage, HttpResponseMessage> responder,
        int tentativas)
    {
        var cliente = new HttpClient(new Manipulador(responder));
        return new ProvedorEmbeddingOpenAi(
            new FabricaHttp(cliente),
            Options.Create(openAi),
            Options.Create(new OpcoesEmbedding
            {
                Model = "modelo-de-teste",
                Dimensao = 2,
                Tentativas = tentativas,
                TimeoutSegundos = 5
            }));
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string corpo) =>
        new(status) { Content = new StringContent(corpo, Encoding.UTF8, "application/json") };

    private sealed class FabricaHttp(HttpClient cliente) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => cliente;
    }

    private sealed class Manipulador(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }
}

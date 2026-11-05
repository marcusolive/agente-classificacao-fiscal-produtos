using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Api.RAG.Tests.integration;

public static class ClienteDocumento
{
    public const string Chave = "segredo-teste";

    public static HttpClient Criar(FabricaApi fabrica)
    {
        var cliente = fabrica.CreateClient();
        cliente.DefaultRequestHeaders.Add("X-Chave-Ingestao", Chave);
        return cliente;
    }

    public static async Task<JsonElement> PostarAsync(HttpClient cliente, object corpo)
    {
        var resposta = await cliente.PostAsJsonAsync("/v1/documentos", corpo);
        var json = await resposta.Content.ReadAsStringAsync();
        Assert.True(resposta.IsSuccessStatusCode, json);
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    public static object Texto(string conteudo, string versao = "1", string inicio = "2026-01-01", string? fim = null) => new
    {
        origem = "oficial",
        identificacao = "ncm",
        versao,
        faixa = "legislacaoVigente",
        vigenciaInicial = inicio,
        vigenciaFinal = fim,
        formato = "texto",
        conteudo
    };

    public static ByteArrayContent Arquivo(string formato, byte[] bytes, string versao = "1")
    {
        var json = JsonSerializer.Serialize(new
        {
            origem = "oficial",
            identificacao = "nota",
            versao,
            faixa = "notasRgi",
            vigenciaInicial = "2026-01-01",
            formato,
            arquivo = Convert.ToBase64String(bytes)
        });
        var conteudo = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
        conteudo.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return conteudo;
    }
}

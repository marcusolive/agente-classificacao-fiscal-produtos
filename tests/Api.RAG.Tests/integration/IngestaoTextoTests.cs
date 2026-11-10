using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Api.RAG.Tests.integration;

public class IngestaoTextoTests : IClassFixture<FabricaApi>
{
    private readonly FabricaApi _fabrica;

    public IngestaoTextoTests(FabricaApi fabrica) => _fabrica = fabrica;

    [Fact]
    public async Task Texto_e_gravado_com_um_chunk_e_o_mesmo_hash_e_ignorado()
    {
        var cliente = ClienteDocumento.Criar(_fabrica);
        var criado = await ClienteDocumento.PostarAsync(cliente, ClienteDocumento.Texto("texto da base"));
        var id = criado.GetProperty("documentoId").GetGuid();

        Assert.Equal("gravado", criado.GetProperty("estado").GetString());
        Assert.Equal("oficial/ncm/1", criado.GetProperty("fonte").GetString());
        Assert.False(criado.GetProperty("consultavel").GetBoolean());

        var lido = await cliente.GetFromJsonAsync<JsonElement>($"/v1/documentos/{id}");
        Assert.Equal("texto da base", lido.GetProperty("chunks")[0].GetProperty("texto").GetString());
        Assert.Equal(64, lido.GetProperty("hash").GetString()!.Length);

        var repetido = await ClienteDocumento.PostarAsync(cliente, ClienteDocumento.Texto("texto da base"));
        Assert.Equal("ignorado", repetido.GetProperty("estado").GetString());
        Assert.Equal(id, repetido.GetProperty("documentoId").GetGuid());
        var deNovo = await cliente.GetFromJsonAsync<JsonElement>($"/v1/documentos/{id}");
        Assert.Equal(1, deNovo.GetProperty("chunks").GetArrayLength());
    }

    [Fact]
    public async Task Conteudo_diferente_na_mesma_versao_e_recusado()
    {
        var cliente = ClienteDocumento.Criar(_fabrica);
        await ClienteDocumento.PostarAsync(cliente, ClienteDocumento.Texto("texto original", "2"));
        var resposta = await cliente.PostAsJsonAsync("/v1/documentos", ClienteDocumento.Texto("texto alterado", "2"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("versão", corpo.GetProperty("erro").GetString(), StringComparison.Ordinal);
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Api.RAG.Tests.integration;

public class RemocaoDocumentoTests : IClassFixture<FabricaApi>
{
    private readonly FabricaApi _fabrica;

    public RemocaoDocumentoTests(FabricaApi fabrica) => _fabrica = fabrica;

    [Fact]
    public async Task Remove_todas_as_versoes_da_origem_com_a_chave()
    {
        var cliente = ClienteDocumento.Criar(_fabrica);
        var a = await ClienteDocumento.PostarAsync(cliente, ClienteDocumento.Texto("primeira versao", "10"));
        await ClienteDocumento.PostarAsync(cliente, ClienteDocumento.Texto("segunda versao", "11"));
        var id = a.GetProperty("documentoId").GetGuid();

        var resposta = await cliente.DeleteAsync("/v1/documentos?origem=oficial");
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.True(corpo.GetProperty("removidos").GetInt32() >= 2);

        var apagado = await cliente.GetAsync($"/v1/documentos/{id}");
        Assert.Equal(HttpStatusCode.NotFound, apagado.StatusCode);
    }

    [Fact]
    public async Task Sem_a_chave_a_remocao_e_recusada()
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.DeleteAsync("/v1/documentos?origem=oficial");
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}

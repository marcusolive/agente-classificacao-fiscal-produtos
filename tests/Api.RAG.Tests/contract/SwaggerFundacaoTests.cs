using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.RAG.Tests.contract;

public class SwaggerFundacaoTests : IClassFixture<integration.FabricaApi>
{
    private readonly integration.FabricaApi _fabrica;

    public SwaggerFundacaoTests(integration.FabricaApi fabrica) => _fabrica = fabrica;

    [Fact]
    public async Task Documento_v1_abre_no_swagger_e_a_raiz_redireciona()
    {
        var cliente = _fabrica.CreateClient();
        var corpo = await cliente.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        var info = corpo.GetProperty("info");

        Assert.Equal("1.0.0", info.GetProperty("version").GetString());
        Assert.Contains("consultar_conhecimento", info.GetProperty("description").GetString(), StringComparison.Ordinal);
        Assert.Contains("não é uma rota REST", info.GetProperty("description").GetString(), StringComparison.Ordinal);
        var caminhos = corpo.GetProperty("paths");
        Assert.True(caminhos.TryGetProperty("/v1/documentos", out _));
        Assert.True(caminhos.TryGetProperty("/v1/documentos/{documentoId}", out _));
        Assert.False(caminhos.TryGetProperty("/mcp", out _));

        var ui = await cliente.GetAsync("/swagger/index.html");
        ui.EnsureSuccessStatusCode();
        Assert.Contains("1.0.0", await ui.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        var script = await cliente.GetAsync("/swagger/index.js");
        script.EnsureSuccessStatusCode();
        Assert.Contains("/openapi/v1.json", await script.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        var semSeguir = _fabrica.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var raiz = await semSeguir.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, raiz.StatusCode);
        Assert.Equal("/swagger", raiz.Headers.Location?.OriginalString);
    }
}

using System.Net.Http.Json;
using System.Text.Json;

namespace Api.Agente.Tests.contract;

public class AnaliseContratoTests : IClassFixture<integration.FabricaApi>
{
    private readonly integration.FabricaApi _fabrica;

    public AnaliseContratoTests(integration.FabricaApi fabrica) => _fabrica = fabrica;

    [Fact]
    public async Task Resposta_e_consulta_usam_o_contrato_1()
    {
        var cliente = _fabrica.CreateClient();
        var criada = await cliente.PostAsJsonAsync("/v1/analises", new
        {
            usuario = "ana",
            empresa = "empresa-1",
            papel = "analista",
            finalidade = "sugerir",
            uf = "SP",
            regimeTributario = "lucro_real",
            tipoOperacao = "saida",
            dataVigencia = "2026-01-01",
            intencao = "sugerirNcm",
            entrada = new
            {
                tipo = "texto",
                texto = """
                    Função: unir peças
                    Composição: aço
                    Apresentação: unidade
                    produto de contrato
                    """
            }
        });
        criada.EnsureSuccessStatusCode();
        var corpo = (await criada.Content.ReadFromJsonAsync<JsonElement>())!;
        Assert.Equal("1.0.0", corpo.GetProperty("contratoVersao").GetString());
        Assert.Equal("1.0.0", corpo.GetProperty("versaoAgente").GetString());
        Assert.True(corpo.TryGetProperty("execucaoId", out var id));
        Assert.True(corpo.TryGetProperty("estadoNota", out _));
        Assert.True(corpo.TryGetProperty("itens", out _));
        Assert.Contains("layout de item não registrado", corpo.GetProperty("validacoesNaoRealizadas").EnumerateArray().Select(v => v.GetString()));

        var consulta = await cliente.GetAsync($"/v1/analises/{id.GetGuid()}");
        consulta.EnsureSuccessStatusCode();
        var obtido = (await consulta.Content.ReadFromJsonAsync<JsonElement>())!;
        Assert.Equal(id.GetGuid(), obtido.GetProperty("execucaoId").GetGuid());
    }

    [Fact]
    public async Task Documento_openapi_publica_somente_a_versao_1()
    {
        var cliente = _fabrica.CreateClient();
        var documento = await cliente.GetAsync("/openapi/v1.json");
        documento.EnsureSuccessStatusCode();
        var corpo = (await documento.Content.ReadFromJsonAsync<JsonElement>())!;
        var info = corpo.GetProperty("info");
        Assert.Equal("1.2.0", info.GetProperty("version").GetString());
        Assert.False(string.IsNullOrWhiteSpace(info.GetProperty("description").GetString()));

        var caminhos = corpo.GetProperty("paths");
        Assert.True(caminhos.TryGetProperty("/v1/analises", out var criar));
        Assert.False(string.IsNullOrWhiteSpace(criar.GetProperty("post").GetProperty("description").GetString()));
        Assert.True(caminhos.TryGetProperty("/v1/analises/{execucaoId}", out var obter));
        Assert.False(string.IsNullOrWhiteSpace(obter.GetProperty("get").GetProperty("summary").GetString()));
        Assert.True(caminhos.TryGetProperty("/v1/analises/{execucaoId}/confirmacoes", out var confirmar));
        Assert.False(string.IsNullOrWhiteSpace(confirmar.GetProperty("post").GetProperty("description").GetString()));
        Assert.False(caminhos.TryGetProperty("/analises", out _));

        var esquemas = corpo.GetProperty("components").GetProperty("schemas");
        Assert.Contains("contrato 1.0.0", esquemas.GetProperty("Execucao").GetProperty("description").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(
            esquemas.GetProperty("Execucao").GetProperty("properties").GetProperty("execucaoId").GetProperty("description").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(
            esquemas.GetProperty("PedidoAnalise").GetProperty("properties").GetProperty("entrada").GetProperty("description").GetString()));

        var ui = await cliente.GetAsync("/swagger/index.html");
        ui.EnsureSuccessStatusCode();
        Assert.Contains("1.2.0", await ui.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        var script = await cliente.GetAsync("/swagger/index.js");
        script.EnsureSuccessStatusCode();
        Assert.Contains("/openapi/v1.json", await script.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }
}

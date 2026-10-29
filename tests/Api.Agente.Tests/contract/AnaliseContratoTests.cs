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
        var criada = await cliente.PostAsJsonAsync("/analises", new
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

        var consulta = await cliente.GetAsync($"/analises/{id.GetGuid()}");
        consulta.EnsureSuccessStatusCode();
        var obtido = (await consulta.Content.ReadFromJsonAsync<JsonElement>())!;
        Assert.Equal(id.GetGuid(), obtido.GetProperty("execucaoId").GetGuid());
    }
}

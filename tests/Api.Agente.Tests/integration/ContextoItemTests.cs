using System.Net.Http.Json;
using System.Text.Json;
using Api.Agente.Domain;

namespace Api.Agente.Tests.integration;

public class ContextoItemTests : IClassFixture<FabricaApi>
{
    private readonly FabricaApi _fabrica;

    public ContextoItemTests(FabricaApi fabrica)
    {
        _fabrica = fabrica;
        _fabrica.Conhecimento.Registrar("parafuso", new Evidencia
        {
            Fonte = "legislacao-janeiro",
            FaixaAutoridade = FaixaAutoridade.LegislacaoVigente,
            Trecho = "Vigencia de janeiro",
            Vigencia = new DateOnly(2026, 1, 1),
            Ncm = "73181500"
        });
        _fabrica.Conhecimento.Registrar("parafuso", new Evidencia
        {
            Fonte = "legislacao-junho",
            FaixaAutoridade = FaixaAutoridade.LegislacaoVigente,
            Trecho = "Vigencia de junho",
            Vigencia = new DateOnly(2026, 6, 1),
            Ncm = "73181600"
        });
    }

    [Fact]
    public async Task Vigencia_e_uf_do_item_prevalecem_sobre_as_da_execucao()
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/v1/analises", new
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
                itens = new object[]
                {
                    new
                    {
                        descricao = ClienteDeAnalise.Completo("parafuso do item"),
                        uf = "RJ",
                        dataVigencia = "2026-06-01",
                        unidade = "UN",
                        ncmDeclarada = "73181500"
                    }
                }
            }
        });
        resposta.EnsureSuccessStatusCode();
        var item = (await resposta.Content.ReadFromJsonAsync<JsonElement>())!.GetProperty("itens")[0];
        var contexto = item.GetProperty("contexto");

        Assert.Equal("73181600", item.GetProperty("ncmSugerida").GetString());
        Assert.Equal("RJ", contexto.GetProperty("uf").GetString());
        Assert.Equal("2026-06-01", contexto.GetProperty("dataVigencia").GetString());
        Assert.Equal("saida", contexto.GetProperty("tipoOperacao").GetString());
        Assert.Equal("lucro_real", contexto.GetProperty("regimeTributario").GetString());
        Assert.Equal("UN", contexto.GetProperty("unidade").GetString());
        Assert.Equal("73181500", contexto.GetProperty("ncmDeclarada").GetString());
        Assert.Equal("legislacao-junho", item.GetProperty("evidencias")[0].GetProperty("fonte").GetString());
    }

    [Fact]
    public async Task Item_sem_contexto_proprio_usa_o_da_execucao()
    {
        var json = await ClienteDeAnalise.PostarAsync(_fabrica.CreateClient(), "sugerirNcm", new
        {
            tipo = "texto",
            texto = ClienteDeAnalise.Completo("parafuso da execucao")
        });
        var contexto = json.GetProperty("itens")[0].GetProperty("contexto");

        Assert.Equal("73181500", json.GetProperty("itens")[0].GetProperty("ncmSugerida").GetString());
        Assert.Equal("SP", contexto.GetProperty("uf").GetString());
        Assert.Equal("2026-01-01", contexto.GetProperty("dataVigencia").GetString());
        Assert.Equal(JsonValueKind.Null, contexto.GetProperty("origem").ValueKind);
    }
}

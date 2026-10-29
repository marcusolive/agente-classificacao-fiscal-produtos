using System.Net.Http.Json;
using System.Text.Json;
using Api.Agente.Domain;

namespace Api.Agente.Tests.integration;

public class ItemIncompletoTests : IClassFixture<FabricaApi>
{
    private static readonly DateOnly Vigencia = new(2026, 1, 1);
    private readonly FabricaApi _fabrica;

    public ItemIncompletoTests(FabricaApi fabrica)
    {
        _fabrica = fabrica;
        _fabrica.Conhecimento.Registrar("parafuso", new Evidencia
        {
            Fonte = "legislacao-teste",
            FaixaAutoridade = FaixaAutoridade.LegislacaoVigente,
            Trecho = "Parafuso de aco no codigo de teste 73181500",
            Vigencia = Vigencia,
            Ncm = "73181500"
        });
    }

    [Fact]
    public async Task Item_incompleto_pede_o_dado_e_nao_bloqueia()
    {
        var json = await ClienteDeAnalise.PostarAsync(_fabrica.CreateClient(), "sugerirNcm", new
        {
            tipo = "texto",
            itens = new object[]
            {
                new { descricao = ClienteDeAnalise.SemComposicao("peca sem composicao") }
            }
        });

        var item = json.GetProperty("itens")[0];
        Assert.Equal("pendenteDeDado", json.GetProperty("estadoNota").GetString());
        Assert.Equal("dadoAusente", item.GetProperty("situacao").GetString());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("ncmSugerida").ValueKind);
        Assert.Contains("composicao", item.GetProperty("dadoAusente").EnumerateArray().Select(v => v.GetString()));
        Assert.NotEqual("divergente", item.GetProperty("situacao").GetString());
    }

    [Fact]
    public async Task Item_completo_segue_quando_o_vizinho_esta_incompleto()
    {
        var json = await ClienteDeAnalise.PostarAsync(_fabrica.CreateClient(), "sugerirNcm", new
        {
            tipo = "texto",
            itens = new object[]
            {
                new { descricao = ClienteDeAnalise.SemComposicao("incompleto") },
                new { descricao = ClienteDeAnalise.Completo("parafuso completo") }
            }
        });

        Assert.Equal("pendenteDeDado", json.GetProperty("estadoNota").GetString());
        Assert.Equal("dadoAusente", json.GetProperty("itens")[0].GetProperty("situacao").GetString());
        Assert.Equal("73181500", json.GetProperty("itens")[1].GetProperty("ncmSugerida").GetString());
    }

    [Fact]
    public async Task Incompleto_continua_sem_classificacao_quando_ha_divergencia()
    {
        var json = await ClienteDeAnalise.PostarAsync(_fabrica.CreateClient(), "auditarDeclarado", new
        {
            tipo = "texto",
            itens = new object[]
            {
                new { descricao = ClienteDeAnalise.SemComposicao("incompleto") },
                new
                {
                    descricao = ClienteDeAnalise.Completo("parafuso divergente"),
                    ncmDeclarada = "99999999"
                }
            }
        });

        Assert.Equal("bloqueada", json.GetProperty("estadoNota").GetString());
        var incompleto = json.GetProperty("itens")[0];
        Assert.Equal("dadoAusente", incompleto.GetProperty("situacao").GetString());
        Assert.Equal(JsonValueKind.Null, incompleto.GetProperty("ncmSugerida").ValueKind);
        Assert.Equal("divergente", json.GetProperty("itens")[1].GetProperty("situacao").GetString());
    }
}

using System.Text.Json;
using Api.Agente.Domain;

namespace Api.Agente.Tests.integration;

public class RevisaoHumanaTests : IClassFixture<FabricaApi>
{
    private static readonly DateOnly Vigencia = new(2026, 1, 1);
    private readonly FabricaApi _fabrica;

    public RevisaoHumanaTests(FabricaApi fabrica)
    {
        _fabrica = fabrica;
        _fabrica.Conhecimento.Registrar("caso-interno", new Evidencia
        {
            Fonte = "caso-confirmado-teste",
            FaixaAutoridade = FaixaAutoridade.CasoCorporativoConfirmado,
            Trecho = "Caso confirmado de teste",
            Vigencia = Vigencia,
            Ncm = "84818099"
        });
        _fabrica.Conhecimento.Registrar("empate-ncm", new Evidencia
        {
            Fonte = "legislacao-a",
            FaixaAutoridade = FaixaAutoridade.LegislacaoVigente,
            Trecho = "Primeira legislacao de teste",
            Vigencia = Vigencia,
            Ncm = "11111111"
        });
        _fabrica.Conhecimento.Registrar("empate-ncm", new Evidencia
        {
            Fonte = "legislacao-b",
            FaixaAutoridade = FaixaAutoridade.LegislacaoVigente,
            Trecho = "Segunda legislacao de teste",
            Vigencia = Vigencia,
            Ncm = "22222222"
        });
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
    public async Task Caso_confirmado_devolve_sugestao_65_e_revisao()
    {
        var item = await ItemAsync(ClienteDeAnalise.Completo("caso-interno de teste"));
        Assert.Equal("84818099", item.GetProperty("ncmSugerida").GetString());
        Assert.Equal(65, item.GetProperty("confianca").GetInt32());
        Assert.True(item.GetProperty("revisaoHumana").GetBoolean());
        Assert.False(item.GetProperty("impactoRelevante").GetBoolean());
    }

    [Fact]
    public async Task Empate_na_mesma_faixa_nao_escolhe_vencedor_nem_bloqueia()
    {
        var cliente = _fabrica.CreateClient();
        var json = await ClienteDeAnalise.PostarAsync(cliente, "sugerirNcm", new
        {
            tipo = "texto",
            texto = ClienteDeAnalise.Completo("empate-ncm de teste")
        });
        var item = json.GetProperty("itens")[0];
        Assert.NotEqual("bloqueada", json.GetProperty("estadoNota").GetString());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("ncmSugerida").ValueKind);
        Assert.Equal(JsonValueKind.Null, item.GetProperty("confianca").ValueKind);
        Assert.True(item.GetProperty("revisaoHumana").GetBoolean());
        Assert.False(item.GetProperty("impactoRelevante").GetBoolean());
    }

    [Fact]
    public async Task Confianca_90_nao_pede_revisao_por_nota_baixa()
    {
        var item = await ItemAsync(ClienteDeAnalise.Completo("parafuso de teste"));
        Assert.Equal(90, item.GetProperty("confianca").GetInt32());
        Assert.False(item.GetProperty("revisaoHumana").GetBoolean());
        Assert.False(item.GetProperty("impactoRelevante").GetBoolean());
    }

    [Fact]
    public async Task Divergencia_bloqueia_e_o_empate_permanece_em_revisao()
    {
        var json = await ClienteDeAnalise.PostarAsync(_fabrica.CreateClient(), "auditarDeclarado", new
        {
            tipo = "texto",
            itens = new object[]
            {
                new { descricao = ClienteDeAnalise.Completo("empate-ncm isolado") },
                new { descricao = ClienteDeAnalise.Completo("parafuso divergente"), ncmDeclarada = "99999999" }
            }
        });

        Assert.Equal("bloqueada", json.GetProperty("estadoNota").GetString());
        Assert.True(json.GetProperty("itens")[0].GetProperty("revisaoHumana").GetBoolean());
        Assert.NotEqual("divergente", json.GetProperty("itens")[0].GetProperty("situacao").GetString());
        Assert.Equal("divergente", json.GetProperty("itens")[1].GetProperty("situacao").GetString());
        Assert.False(json.GetProperty("itens")[0].GetProperty("impactoRelevante").GetBoolean());
    }

    private async Task<JsonElement> ItemAsync(string texto)
    {
        var json = await ClienteDeAnalise.PostarAsync(_fabrica.CreateClient(), "sugerirNcm", new { tipo = "texto", texto });
        Assert.Equal("sugestaoLiberada", json.GetProperty("estadoNota").GetString());
        return json.GetProperty("itens")[0];
    }
}

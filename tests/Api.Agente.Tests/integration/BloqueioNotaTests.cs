using System.Net.Http.Json;
using System.Text.Json;
using Api.Agente.Domain;

namespace Api.Agente.Tests.integration;

public class BloqueioNotaTests : IClassFixture<FabricaApi>
{
    private static readonly DateOnly Vigencia = new(2026, 1, 1);
    private readonly FabricaApi _fabrica;

    public BloqueioNotaTests(FabricaApi fabrica)
    {
        _fabrica = fabrica;
        _fabrica.Conhecimento.Registrar("parafuso", new Evidencia
        {
            Fonte = "legislacao-teste",
            FaixaAutoridade = FaixaAutoridade.LegislacaoVigente,
            Trecho = "Parafuso de aco no codigo de teste 73181500",
            Vigencia = Vigencia,
            Ncm = "73181500",
            Tributos = new Dictionary<string, string> { ["ipi"] = "previsto na evidencia de teste" }
        });
    }

    [Fact]
    public async Task Ncm_declarada_diferente_bloqueia_a_nota()
    {
        var json = await AuditarAsync(
        [
            Item("parafuso divergente", "99999999", "outro valor")
        ]);

        var item = json.GetProperty("itens")[0];
        Assert.Equal("bloqueada", json.GetProperty("estadoNota").GetString());
        Assert.Equal("divergente", item.GetProperty("situacao").GetString());
        Assert.Equal("73181500", item.GetProperty("ncmSugerida").GetString());
        Assert.Contains("NCM", item.GetProperty("incorreto").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("inconsistenciaNegocio", item.GetProperty("achados")[0].GetProperty("classe").GetString());
        Assert.DoesNotContain("conformidade", json.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Declaracao_igual_a_evidencia_nao_diverge()
    {
        var json = await AuditarAsync(
        [
            Item("parafuso compativel", "7318.15.00", "previsto na evidencia de teste")
        ]);

        var item = json.GetProperty("itens")[0];
        Assert.Equal("sugestaoLiberada", json.GetProperty("estadoNota").GetString());
        Assert.Equal("sugerido", item.GetProperty("situacao").GetString());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("incorreto").ValueKind);
    }

    [Fact]
    public async Task Um_item_divergente_bloqueia_e_preserva_o_compativel()
    {
        var json = await AuditarAsync(
        [
            Item("parafuso compativel", "73181500", "previsto na evidencia de teste"),
            Item("parafuso divergente", "12345678", "previsto na evidencia de teste")
        ]);

        Assert.Equal("bloqueada", json.GetProperty("estadoNota").GetString());
        Assert.Equal("sugerido", json.GetProperty("itens")[0].GetProperty("situacao").GetString());
        Assert.Equal("divergente", json.GetProperty("itens")[1].GetProperty("situacao").GetString());
    }

    private async Task<JsonElement> AuditarAsync(object[] itens)
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/v1/analises", new
        {
            usuario = "ana",
            empresa = "empresa-1",
            papel = "analista",
            finalidade = "auditar",
            uf = "SP",
            regimeTributario = "lucro_real",
            tipoOperacao = "saida",
            dataVigencia = "2026-01-01",
            intencao = "auditarDeclarado",
            entrada = new { tipo = "texto", itens }
        });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<JsonElement>())!;
    }

    private static object Item(string descricao, string ncm, string ipi) => new
    {
        descricao = Texto(descricao),
        ncmDeclarada = ncm,
        tributosDeclarados = new Dictionary<string, string> { ["ipi"] = ipi }
    };

    private static string Texto(string descricao) =>
        $"""
        Função: unir peças
        Composição: aço
        Apresentação: unidade
        {descricao}
        """;
}

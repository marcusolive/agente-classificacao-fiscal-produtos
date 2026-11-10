using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Agente.Domain;

namespace Api.Agente.Tests.integration;

public class SugestaoTextoTests : IClassFixture<FabricaApi>
{
    private static readonly DateOnly Vigencia = new(2026, 1, 1);
    private readonly FabricaApi _fabrica;

    public SugestaoTextoTests(FabricaApi fabrica)
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
        _fabrica.Conhecimento.Registrar("parafuso", new Evidencia
        {
            Fonte = "fornecedor-teste",
            FaixaAutoridade = FaixaAutoridade.TextoFornecedor,
            Trecho = "Fornecedor indica outro codigo",
            Vigencia = Vigencia,
            Ncm = "73269090"
        });
    }

    [Fact]
    public async Task Legislacao_vence_o_fornecedor_e_deixa_o_xml_vazio()
    {
        var json = await PostAsync(Texto("parafuso de aco"));
        var item = json.GetProperty("itens")[0];
        Assert.Equal("sugestaoLiberada", json.GetProperty("estadoNota").GetString());
        Assert.Equal("73181500", item.GetProperty("ncmSugerida").GetString());
        Assert.Equal(90, item.GetProperty("confianca").GetInt32());
        Assert.False(item.GetProperty("revisaoHumana").GetBoolean());
        Assert.False(item.GetProperty("impactoRelevante").GetBoolean());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("xmlItem").ValueKind);
        Assert.Equal("legislacaoVigente", item.GetProperty("evidencias")[0].GetProperty("faixaAutoridade").GetString());
        Assert.Equal("sustentado", item.GetProperty("tratamento").GetProperty("ipi").GetProperty("situacao").GetString());
        Assert.Equal("limitacao", item.GetProperty("tratamento").GetProperty("icms").GetProperty("situacao").GetString());
        Assert.Contains("73269090", item.GetProperty("alternativas").EnumerateArray().Select(a => a.GetString()));
    }

    [Fact]
    public async Task Sem_evidencia_nao_escolhe_ncm_nem_bloqueia()
    {
        var json = await PostAsync(Texto("peca sem fonte cadastrada"));
        var item = json.GetProperty("itens")[0];
        Assert.Equal("semEvidencia", item.GetProperty("situacao").GetString());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("ncmSugerida").ValueKind);
        Assert.NotEqual("bloqueada", json.GetProperty("estadoNota").GetString());
        Assert.Equal("ausenciaEvidencias", item.GetProperty("achados")[0].GetProperty("classe").GetString());
    }

    [Fact]
    public async Task Contexto_incompleto_e_recusado()
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/v1/analises", new { entrada = new { tipo = "texto", texto = "x" } });
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    private async Task<JsonElement> PostAsync(string texto)
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
            entrada = new { tipo = "texto", texto }
        });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<JsonElement>())!;
    }

    private static string Texto(string descricao) =>
        $"""
        Função: unir peças
        Composição: aço
        Apresentação: unidade
        {descricao}
        """;
}

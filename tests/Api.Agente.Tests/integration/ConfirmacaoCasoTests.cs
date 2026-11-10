using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Agente.Domain;

namespace Api.Agente.Tests.integration;

public class ConfirmacaoCasoTests : IClassFixture<FabricaApi>
{
    private static readonly DateOnly Vigencia = new(2026, 1, 1);
    private readonly FabricaApi _fabrica;

    public ConfirmacaoCasoTests(FabricaApi fabrica)
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
    public async Task Aceite_devolve_a_ncm_sustentada_sem_gravar_caso()
    {
        var cliente = _fabrica.CreateClient();
        var analise = await ClienteDeAnalise.PostarAsync(cliente, "sugerirNcm", new
        {
            tipo = "texto",
            texto = ClienteDeAnalise.Completo("parafuso de teste")
        });
        var id = analise.GetProperty("execucaoId").GetGuid();

        var confirmacao = await ConfirmarAsync(cliente, id, new
        {
            papelAutoridade = "revisor fiscal",
            itens = new object[] { new { itemId = "1", decisao = "aceitar" } }
        });

        Assert.Equal("1.1.0", confirmacao.GetProperty("contratoVersao").GetString());
        Assert.Equal("empresa-1", confirmacao.GetProperty("empresa").GetString());
        Assert.Equal("revisor fiscal", confirmacao.GetProperty("papelAutoridade").GetString());
        Assert.False(confirmacao.GetProperty("gravado").GetBoolean());
        Assert.Equal("73181500", confirmacao.GetProperty("itens")[0].GetProperty("ncm").GetString());
        Assert.Contains(
            "gravação na base de conhecimento não realizada",
            confirmacao.GetProperty("validacoesNaoRealizadas").EnumerateArray().Select(v => v.GetString()));
        Assert.DoesNotContain("conformidade", confirmacao.ToString(), StringComparison.OrdinalIgnoreCase);

        var consulta = await cliente.GetFromJsonAsync<JsonElement>($"/v1/analises/{id}");
        Assert.Equal("73181500", consulta.GetProperty("itens")[0].GetProperty("ncmSugerida").GetString());
        Assert.False(consulta.TryGetProperty("gravado", out _));
    }

    [Fact]
    public async Task Correcao_aceita_ncm_que_ja_esta_nas_evidencias()
    {
        var cliente = _fabrica.CreateClient();
        var id = await CriarAsync(cliente);
        var confirmacao = await ConfirmarAsync(cliente, id, new
        {
            papelAutoridade = "revisor fiscal",
            itens = new object[] { new { itemId = "1", decisao = "corrigir", ncm = "7326.90.90" } }
        });

        Assert.False(confirmacao.GetProperty("gravado").GetBoolean());
        Assert.Equal("73269090", confirmacao.GetProperty("itens")[0].GetProperty("ncm").GetString());
        Assert.Equal("corrigir", confirmacao.GetProperty("itens")[0].GetProperty("decisao").GetString());
    }

    [Fact]
    public async Task Correcao_recusa_codigo_ausente_das_evidencias()
    {
        var cliente = _fabrica.CreateClient();
        var id = await CriarAsync(cliente);
        var resposta = await cliente.PostAsJsonAsync($"/v1/analises/{id}/confirmacoes", new
        {
            papelAutoridade = "revisor fiscal",
            itens = new object[] { new { itemId = "1", decisao = "corrigir", ncm = "00000000" } }
        });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var depois = await cliente.GetFromJsonAsync<JsonElement>($"/v1/analises/{id}");
        Assert.Equal("73181500", depois.GetProperty("itens")[0].GetProperty("ncmSugerida").GetString());
    }

    [Fact]
    public async Task Papel_ausente_e_execucao_inexistente_sao_recusados()
    {
        var cliente = _fabrica.CreateClient();
        var id = await CriarAsync(cliente);
        var semPapel = await cliente.PostAsJsonAsync($"/v1/analises/{id}/confirmacoes", new
        {
            papelAutoridade = " ",
            itens = new object[] { new { itemId = "1", decisao = "aceitar" } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, semPapel.StatusCode);

        var inexistente = await cliente.PostAsJsonAsync($"/v1/analises/{Guid.NewGuid()}/confirmacoes", new
        {
            papelAutoridade = "revisor fiscal",
            itens = new object[] { new { itemId = "1", decisao = "aceitar" } }
        });
        Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
    }

    private static async Task<Guid> CriarAsync(HttpClient cliente)
    {
        var analise = await ClienteDeAnalise.PostarAsync(cliente, "sugerirNcm", new
        {
            tipo = "texto",
            texto = ClienteDeAnalise.Completo("parafuso de teste")
        });
        return analise.GetProperty("execucaoId").GetGuid();
    }

    private static async Task<JsonElement> ConfirmarAsync(HttpClient cliente, Guid id, object pedido)
    {
        var resposta = await cliente.PostAsJsonAsync($"/v1/analises/{id}/confirmacoes", pedido);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<JsonElement>())!;
    }
}

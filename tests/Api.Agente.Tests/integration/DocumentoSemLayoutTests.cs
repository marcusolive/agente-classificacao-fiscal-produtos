using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace Api.Agente.Tests.integration;

public class DocumentoSemLayoutTests : IClassFixture<FabricaApi>
{
    private readonly FabricaApi _fabrica;

    public DocumentoSemLayoutTests(FabricaApi fabrica) => _fabrica = fabrica;

    [Fact]
    public async Task Arquivo_sem_layout_devolve_erro_estrutural_e_nao_bloqueia()
    {
        var cliente = _fabrica.WithWebHostBuilder(b => b.UseSetting("Analise:TamanhoMaximoBytes", "1048576")).CreateClient();
        var resposta = await PostarArquivoAsync(cliente, "pdf", Convert.ToBase64String("arquivo-de-teste"u8.ToArray()));
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = (await resposta.Content.ReadFromJsonAsync<JsonElement>())!;
        Assert.Equal("erroEstrutural", corpo.GetProperty("classe").GetString());
        Assert.Contains("Layout não registrado", corpo.GetProperty("erro").GetString());
        Assert.False(corpo.TryGetProperty("estadoNota", out _));
    }

    [Fact]
    public async Task Arquivo_vazio_devolve_erro_estrutural()
    {
        var resposta = await PostarArquivoAsync(_fabrica.CreateClient(), "pdf", "");
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = (await resposta.Content.ReadFromJsonAsync<JsonElement>())!;
        Assert.Equal("erroEstrutural", corpo.GetProperty("classe").GetString());
        Assert.False(corpo.TryGetProperty("estadoNota", out _));
    }

    [Fact]
    public async Task Tipo_desconhecido_e_recusado_sem_bloqueio()
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/v1/analises", new
        {
            usuario = "ana",
            empresa = "empresa-1",
            papel = "analista",
            finalidade = "analise",
            uf = "SP",
            regimeTributario = "lucro_real",
            tipoOperacao = "saida",
            dataVigencia = "2026-01-01",
            intencao = "sugerirNcm",
            entrada = new { tipo = "desconhecido", texto = "x" }
        });
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.DoesNotContain("bloqueada", await resposta.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Arquivo_sem_limite_configurado_e_recusado_sem_bloqueio()
    {
        var resposta = await PostarArquivoAsync(_fabrica.CreateClient(), "planilha", Convert.ToBase64String("planilha"u8.ToArray()));
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("Limite de arquivo não configurado", corpo);
        Assert.DoesNotContain("bloqueada", corpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Sugestao_de_xml_nao_monta_nfe_completa()
    {
        var json = await ClienteDeAnalise.PostarAsync(_fabrica.CreateClient(), "sugerirXmlItem", new
        {
            tipo = "texto",
            texto = ClienteDeAnalise.Completo("parafuso sem layout de item")
        });
        Assert.Equal(JsonValueKind.Null, json.GetProperty("itens")[0].GetProperty("xmlItem").ValueKind);
        var texto = json.ToString();
        Assert.DoesNotContain("<NFe", texto, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("infNFe", texto, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("layout de item não registrado", json.GetProperty("validacoesNaoRealizadas").EnumerateArray().Select(v => v.GetString()));
    }

    private static Task<HttpResponseMessage> PostarArquivoAsync(HttpClient cliente, string tipo, string conteudo) =>
        cliente.PostAsJsonAsync("/v1/analises", new
        {
            usuario = "ana",
            empresa = "empresa-1",
            papel = "analista",
            finalidade = "analise",
            uf = "SP",
            regimeTributario = "lucro_real",
            tipoOperacao = "saida",
            dataVigencia = "2026-01-01",
            intencao = "sugerirNcm",
            entrada = new { tipo, arquivo = new { nome = "arquivo", conteudoBase64 = conteudo } }
        });
}

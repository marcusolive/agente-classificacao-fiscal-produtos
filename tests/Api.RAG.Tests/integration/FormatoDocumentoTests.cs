using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace Api.RAG.Tests.integration;

public class FormatoDocumentoTests : IClassFixture<FabricaApi>
{
    private readonly FabricaApi _fabrica;

    public FormatoDocumentoTests(FabricaApi fabrica) => _fabrica = fabrica;

    [Fact]
    public async Task Formato_fora_da_lista_e_recusado()
    {
        var cliente = ClienteDocumento.Criar(_fabrica);
        var resposta = await cliente.PostAsJsonAsync("/v1/documentos", new
        {
            origem = "oficial",
            identificacao = "nota",
            versao = "1",
            faixa = "notasRgi",
            vigenciaInicial = "2026-01-01",
            formato = "planilha",
            conteudo = "nao"
        });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Pdf_com_limite_vira_texto()
    {
        var cliente = _fabrica.WithWebHostBuilder(b => b.UseSetting("Ingestao:TamanhoMaximoBytes", "1000000")).CreateClient();
        cliente.DefaultRequestHeaders.Add("X-Chave-Ingestao", ClienteDocumento.Chave);
        var resposta = await cliente.PostAsync("/v1/documentos", ClienteDocumento.Arquivo("pdf", PdfComTexto(), "5"));
        var json = await resposta.Content.ReadAsStringAsync();
        Assert.True(resposta.IsSuccessStatusCode, json);
        var id = JsonDocument.Parse(json).RootElement.GetProperty("documentoId").GetGuid();
        var lido = await cliente.GetFromJsonAsync<JsonElement>($"/v1/documentos/{id}");
        Assert.Contains("Texto do pdf", lido.GetProperty("chunks")[0].GetProperty("texto").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Pdf_sem_limite_nao_grava_chunk()
    {
        var cliente = ClienteDocumento.Criar(_fabrica);
        var resposta = await cliente.PostAsync("/v1/documentos", ClienteDocumento.Arquivo("pdf", PdfComTexto()));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Limite de arquivo não configurado.", corpo.GetProperty("erro").GetString());
    }

    [Fact]
    public async Task Xml_de_notas_vira_texto_e_xml_vazio_e_recusado()
    {
        var cliente = _fabrica.WithWebHostBuilder(b => b.UseSetting("Ingestao:TamanhoMaximoBytes", "1000000")).CreateClient();
        cliente.DefaultRequestHeaders.Add("X-Chave-Ingestao", ClienteDocumento.Chave);
        var xml = ClienteDocumento.Arquivo("xmlNotas", Encoding.UTF8.GetBytes("<nota>Texto da nota explicativa</nota>"), "3");
        var resposta = await cliente.PostAsync("/v1/documentos", xml);
        var json = await resposta.Content.ReadAsStringAsync();
        Assert.True(resposta.IsSuccessStatusCode, json);
        var id = JsonDocument.Parse(json).RootElement.GetProperty("documentoId").GetGuid();
        var lido = await cliente.GetFromJsonAsync<JsonElement>($"/v1/documentos/{id}");
        Assert.Contains("Texto da nota explicativa", lido.GetProperty("chunks")[0].GetProperty("texto").GetString(), StringComparison.Ordinal);

        var vazio = await cliente.PostAsync("/v1/documentos", ClienteDocumento.Arquivo("xmlNotas", "<nota></nota>"u8.ToArray(), "4"));
        Assert.Equal(HttpStatusCode.BadRequest, vazio.StatusCode);
    }

    private static byte[] PdfComTexto()
    {
        var builder = new PdfDocumentBuilder();
        var pagina = builder.AddPage(PageSize.A4);
        var fonte = builder.AddStandard14Font(Standard14Font.Helvetica);
        pagina.AddText("Texto do pdf", 12, new PdfPoint(25, 700), fonte);
        return builder.Build();
    }
}

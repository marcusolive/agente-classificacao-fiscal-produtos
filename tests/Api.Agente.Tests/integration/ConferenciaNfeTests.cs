using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Api.Agente.Domain;

namespace Api.Agente.Tests.integration;

public class ConferenciaNfeTests : IClassFixture<FabricaApi>
{
    private static readonly DateOnly Vigencia = new(2026, 1, 1);
    private readonly FabricaApi _fabrica;

    public ConferenciaNfeTests(FabricaApi fabrica)
    {
        _fabrica = fabrica;
        _fabrica.Conhecimento.Registrar("Café torrado", new Evidencia
        {
            Fonte = "Classif/Siscomex/0901.21.00",
            FaixaAutoridade = FaixaAutoridade.LegislacaoVigente,
            Trecho = "Café torrado > Não descafeinado",
            Vigencia = Vigencia,
            Ncm = "09012100"
        });
        _fabrica.Conhecimento.Registrar("Açúcar cristal", new Evidencia
        {
            Fonte = "Classif/Siscomex/1701.14.00",
            FaixaAutoridade = FaixaAutoridade.LegislacaoVigente,
            Trecho = "Outros açúcares de cana",
            Vigencia = Vigencia,
            Ncm = "17011400"
        });
    }

    private const string Nfe = """
    <?xml version="1.0" encoding="UTF-8"?>
    <nfeProc versao="4.00" xmlns="http://www.portalfiscal.inf.br/nfe">
      <NFe><infNFe versao="4.00" Id="NFe1">
        <det nItem="1"><prod>
          <xProd>Café torrado e moído, não descafeinado, pacote 500g</xProd>
          <NCM>09012100</NCM><CFOP>5102</CFOP>
        </prod></det>
        <det nItem="2"><prod>
          <xProd>Açúcar cristal de cana, sacaria 1kg</xProd>
          <NCM>84713012</NCM><CFOP>5102</CFOP>
        </prod></det>
      </infNFe></NFe>
    </nfeProc>
    """;

    [Fact]
    public async Task Le_a_nfe_classifica_pela_descricao_e_bloqueia_o_ncm_errado()
    {
        var json = await AuditarAsync(Nfe);

        var itens = json.GetProperty("itens");
        Assert.Equal(2, itens.GetArrayLength());

        // Item 1: café com NCM declarada correta → sugerido, sem divergência.
        var cafe = itens[0];
        Assert.Equal("Café torrado e moído, não descafeinado, pacote 500g", cafe.GetProperty("textoOriginal").GetString());
        Assert.Equal("sugerido", cafe.GetProperty("situacao").GetString());
        Assert.Equal("09012100", cafe.GetProperty("ncmSugerida").GetString());

        // Item 2: açúcar com NCM declarada de informática → divergente, bloqueia a nota.
        var acucar = itens[1];
        Assert.Equal("divergente", acucar.GetProperty("situacao").GetString());
        Assert.Equal("17011400", acucar.GetProperty("ncmSugerida").GetString());
        Assert.Equal("bloqueada", json.GetProperty("estadoNota").GetString());
    }

    [Fact]
    public async Task Arquivo_ausente_em_xmlnfe_e_recusado()
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/v1/analises", new
        {
            usuario = "ana",
            empresa = "empresa-1",
            papel = "fiscal",
            finalidade = "auditar",
            uf = "SP",
            regimeTributario = "lucroReal",
            tipoOperacao = "saida",
            dataVigencia = "2026-01-01",
            intencao = "auditarDeclarado",
            entrada = new { tipo = "xmlNfe" }
        });
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    private async Task<JsonElement> AuditarAsync(string xml)
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/v1/analises", new
        {
            usuario = "ana",
            empresa = "empresa-1",
            papel = "fiscal",
            finalidade = "auditar",
            uf = "SP",
            regimeTributario = "lucroReal",
            tipoOperacao = "saida",
            dataVigencia = "2026-01-01",
            intencao = "auditarDeclarado",
            entrada = new
            {
                tipo = "xmlNfe",
                arquivo = new
                {
                    nome = "nfe.xml",
                    conteudoBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml))
                }
            }
        });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<JsonElement>())!;
    }
}

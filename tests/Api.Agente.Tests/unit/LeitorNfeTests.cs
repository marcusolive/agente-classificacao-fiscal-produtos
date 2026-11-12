using System.Text;
using Api.Agente.Application;
using Api.Agente.Domain;

namespace Api.Agente.Tests.unit;

public class LeitorNfeTests
{
    private const string Nfe = """
    <?xml version="1.0" encoding="UTF-8"?>
    <nfeProc versao="4.00" xmlns="http://www.portalfiscal.inf.br/nfe">
      <NFe><infNFe versao="4.00" Id="NFe1">
        <det nItem="1"><prod>
          <xProd>Cafe torrado e moido, nao descafeinado, 500g</xProd>
          <NCM>09012100</NCM><CFOP>5102</CFOP><CEST>1700100</CEST>
        </prod></det>
        <det nItem="2"><prod>
          <xProd>Acucar cristal de cana, 1kg</xProd>
          <NCM>84713012</NCM><CFOP>5102</CFOP>
        </prod></det>
      </infNFe></NFe>
    </nfeProc>
    """;

    private static byte[] Bytes(string xml) => Encoding.UTF8.GetBytes(xml);

    [Fact]
    public void Extrai_um_item_por_produto_com_ncm_declarada()
    {
        var itens = LeitorNfe.Ler(Bytes(Nfe));

        Assert.Equal(2, itens.Count);
        Assert.Equal("Cafe torrado e moido, nao descafeinado, 500g", itens[0].Descricao);
        Assert.Equal("09012100", itens[0].NcmDeclarada);
        Assert.Equal("5102", itens[0].Cfop);
        Assert.Equal("1700100", itens[0].CestDeclarado);
        Assert.Equal("84713012", itens[1].NcmDeclarada);
        Assert.Null(itens[1].CestDeclarado);
    }

    [Fact]
    public void Xml_ilegivel_e_recusado()
    {
        var ex = Assert.Throws<EntradaRejeitadaException>(() => LeitorNfe.Ler(Bytes("isto nao e xml")));
        Assert.Equal(ClasseAchado.ErroEstrutural, ex.Classe);
    }

    [Fact]
    public void Nota_sem_itens_e_recusada()
    {
        var vazia = "<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\"><NFe><infNFe/></NFe></nfeProc>";
        var ex = Assert.Throws<EntradaRejeitadaException>(() => LeitorNfe.Ler(Bytes(vazia)));
        Assert.Equal(ClasseAchado.ErroEstrutural, ex.Classe);
    }
}

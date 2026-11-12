using System.Xml;
using System.Xml.Linq;
using Api.Agente.Domain;

namespace Api.Agente.Application;

/// <summary>
/// Le uma NF-e (modelo 55) e extrai um item por produto, com a descricao (xProd) e
/// o que foi declarado (NCM, CFOP, CEST). E dado para conferencia, nao instrucao.
/// A analise de NCM continua sendo feita pelo fluxo normal, sobre a descricao.
/// </summary>
public static class LeitorNfe
{
    public static List<ItemPedido> Ler(byte[] conteudo)
    {
        XDocument documento;
        try
        {
            // DtdProcessing.Prohibit + XmlResolver nulo: sem entidades externas (XXE).
            var ajustes = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
            using var fluxo = new MemoryStream(conteudo);
            using var leitor = XmlReader.Create(fluxo, ajustes);
            documento = XDocument.Load(leitor);
        }
        catch (XmlException)
        {
            throw new EntradaRejeitadaException("Arquivo XML ilegível.", ClasseAchado.ErroEstrutural);
        }

        var produtos = documento.Descendants()
            .Where(e => e.Name.LocalName == "det")
            .Select(det => det.Elements().FirstOrDefault(e => e.Name.LocalName == "prod"))
            .Where(prod => prod is not null)
            .Cast<XElement>()
            .ToList();

        if (produtos.Count == 0)
        {
            throw new EntradaRejeitadaException("Nota sem itens (prod) para conferir.", ClasseAchado.ErroEstrutural);
        }

        var itens = new List<ItemPedido>();
        foreach (var prod in produtos)
        {
            var descricao = Valor(prod, "xProd");
            if (string.IsNullOrWhiteSpace(descricao))
            {
                continue;
            }

            itens.Add(new ItemPedido
            {
                Descricao = descricao,
                NcmDeclarada = Valor(prod, "NCM"),
                CestDeclarado = Valor(prod, "CEST"),
                Cfop = Valor(prod, "CFOP")
            });
        }

        if (itens.Count == 0)
        {
            throw new EntradaRejeitadaException("Nota sem descrição de produto (xProd) para conferir.", ClasseAchado.ErroEstrutural);
        }

        return itens;
    }

    private static string? Valor(XElement prod, string nome)
    {
        var elemento = prod.Elements().FirstOrDefault(e => e.Name.LocalName == nome);
        return string.IsNullOrWhiteSpace(elemento?.Value) ? null : elemento!.Value.Trim();
    }
}

using System.Text;
using System.Xml.Linq;
using Api.RAG.Domain;
using UglyToad.PdfPig;

namespace Api.RAG.Application;

public sealed class ExtratorTexto
{
    public string Extrair(FormatoDocumento formato, byte[] conteudo)
    {
        var texto = formato switch
        {
            FormatoDocumento.Texto => Encoding.UTF8.GetString(conteudo),
            FormatoDocumento.Pdf => TextoPdf(conteudo),
            FormatoDocumento.XmlNotas => TextoXml(conteudo),
            _ => throw new RecusaIngestaoException("Formato de documento não aceito.")
        };

        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new RecusaIngestaoException("Arquivo sem texto identificável.");
        }

        return texto.Trim();
    }

    private static string TextoPdf(byte[] conteudo)
    {
        try
        {
            using var documento = PdfDocument.Open(conteudo);
            return string.Join('\n', documento.GetPages().Select(pagina => pagina.Text));
        }
        catch (RecusaIngestaoException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new RecusaIngestaoException("Arquivo ilegível.");
        }
    }

    private static string TextoXml(byte[] conteudo)
    {
        try
        {
            var xml = XDocument.Parse(Encoding.UTF8.GetString(conteudo));
            return string.Concat(xml.DescendantNodes().OfType<XText>().Select(no => no.Value));
        }
        catch (Exception)
        {
            throw new RecusaIngestaoException("Arquivo ilegível.");
        }
    }
}

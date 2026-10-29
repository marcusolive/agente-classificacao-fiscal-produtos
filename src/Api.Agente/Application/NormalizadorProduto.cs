using System.Globalization;
using System.Text;
using Api.Agente.Domain;

namespace Api.Agente.Application;

public static class NormalizadorProduto
{
    public static AtributosProduto Normalizar(string texto)
    {
        string? funcao = null, material = null, composicao = null, uso = null, apresentacao = null, termos = null;
        foreach (var linha in texto.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separador = linha.IndexOf(':');
            if (separador <= 0)
            {
                continue;
            }

            var chave = Simplificar(linha[..separador]);
            var valor = linha[(separador + 1)..].Trim();
            switch (chave)
            {
                case "funcao": funcao = valor; break;
                case "material": material = valor; break;
                case "composicao": composicao = valor; break;
                case "uso": uso = valor; break;
                case "apresentacao": apresentacao = valor; break;
                case "termos": termos = valor; break;
            }
        }

        return new AtributosProduto
        {
            Funcao = funcao,
            Material = material,
            Composicao = composicao,
            Uso = uso,
            Apresentacao = apresentacao,
            TermosTecnicos = termos
        };
    }

    public static IReadOnlyList<string> DadosEssenciaisAusentes(AtributosProduto atributos)
    {
        var ausentes = new List<string>();
        if (string.IsNullOrWhiteSpace(atributos.Funcao)) ausentes.Add("funcao");
        if (string.IsNullOrWhiteSpace(atributos.Composicao)) ausentes.Add("composicao");
        if (string.IsNullOrWhiteSpace(atributos.Apresentacao)) ausentes.Add("apresentacao");
        return ausentes;
    }

    private static string Simplificar(string texto)
    {
        var semAcento = texto.Normalize(NormalizationForm.FormD);
        var letras = semAcento.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return string.Concat(letras).ToLowerInvariant().Trim();
    }
}

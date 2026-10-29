using Api.Agente.Domain;

namespace Api.Agente.Application;

public static class ComparadorDeclaracao
{
    public static void Aplicar(ItemAnalise item, string? ncmDeclarada, IReadOnlyDictionary<string, string>? tributosDeclarados)
    {
        if (item.Situacao != SituacaoItem.Sugerido || string.IsNullOrWhiteSpace(item.NcmSugerida))
        {
            return;
        }

        var divergencias = new List<string>();
        if (!string.IsNullOrWhiteSpace(ncmDeclarada) && !MesmaNcm(ncmDeclarada, item.NcmSugerida))
        {
            divergencias.Add($"NCM declarada {SomenteDigitos(ncmDeclarada)}");
        }

        if (tributosDeclarados is not null)
        {
            foreach (var (tributo, declarado) in tributosDeclarados)
            {
                if (string.IsNullOrWhiteSpace(declarado))
                {
                    continue;
                }

                if (!item.Tratamento.TryGetValue(tributo, out var sustentado) || sustentado.Situacao != SituacaoTratamento.Sustentado)
                {
                    continue;
                }

                if (!string.Equals(declarado.Trim(), sustentado.Valor, StringComparison.OrdinalIgnoreCase))
                {
                    divergencias.Add($"tratamento de {tributo} declarado");
                }
            }
        }

        if (divergencias.Count == 0)
        {
            return;
        }

        item.Situacao = SituacaoItem.Divergente;
        item.Incorreto = string.Join("; ", divergencias);
        item.Achados.Add(new Achado
        {
            Classe = ClasseAchado.InconsistenciaNegocio,
            Severidade = SeveridadeAchado.Erro,
            Descricao = $"O declarado não se sustenta na evidência. Classificação sustentada: {item.NcmSugerida}."
        });
    }

    private static bool MesmaNcm(string declarada, string sugerida) =>
        SomenteDigitos(declarada) == SomenteDigitos(sugerida);

    private static string SomenteDigitos(string codigo) =>
        string.Concat(codigo.Where(char.IsDigit));
}

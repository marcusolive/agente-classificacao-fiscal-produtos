namespace Api.Agente.Domain;

public sealed record ResultadoOrdenacao(IReadOnlyList<Evidencia> Ordenadas, bool Empate, Evidencia? Vencedora);

public static class OrdenacaoDeEvidencias
{
    public static ResultadoOrdenacao Ordenar(IEnumerable<Evidencia> evidencias)
    {
        var comNcm = evidencias
            .Where(e => !string.IsNullOrWhiteSpace(e.Ncm))
            .OrderBy(e => e.FaixaAutoridade)
            .ToList();

        if (comNcm.Count == 0)
        {
            return new ResultadoOrdenacao(comNcm, false, null);
        }

        var faixa = comNcm[0].FaixaAutoridade;
        var daFaixa = comNcm.Where(e => e.FaixaAutoridade == faixa).Select(e => e.Ncm).Distinct().ToList();
        if (daFaixa.Count > 1)
        {
            return new ResultadoOrdenacao(comNcm, true, null);
        }

        return new ResultadoOrdenacao(comNcm, false, comNcm[0]);
    }
}

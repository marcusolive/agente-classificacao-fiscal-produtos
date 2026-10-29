namespace Api.Agente.Domain;

public static class CalculadoraEstadoNota
{
    public static EstadoNota Calcular(IEnumerable<ItemAnalise> itens)
    {
        var lista = itens.ToList();
        if (lista.Any(i => i.Situacao == SituacaoItem.Divergente))
        {
            return EstadoNota.Bloqueada;
        }

        if (lista.Any(i => i.Situacao == SituacaoItem.DadoAusente))
        {
            return EstadoNota.PendenteDeDado;
        }

        if (lista.Any(i => i.Situacao == SituacaoItem.FalhaTecnica))
        {
            return EstadoNota.FalhaTecnica;
        }

        return EstadoNota.SugestaoLiberada;
    }
}

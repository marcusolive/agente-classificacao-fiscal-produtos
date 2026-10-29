using Api.Agente.Domain;

namespace Api.Agente.Application;

public sealed class AnalisadorDeterministico : IAnalisadorModelo
{
    public Task<SaidaModelo> AnalisarAsync(ContextoModelo contexto, CancellationToken ct)
    {
        return Task.FromResult(new SaidaModelo
        {
            Ncm = contexto.Vencedora.Ncm,
            Explicacao = contexto.Vencedora.Trecho
        });
    }
}

public static class ValidadorSaidaModelo
{
    public static bool NcmPermitida(string? ncm, IEnumerable<Evidencia> evidencias)
    {
        if (string.IsNullOrWhiteSpace(ncm))
        {
            return false;
        }

        return evidencias.Any(e => string.Equals(e.Ncm, ncm, StringComparison.Ordinal));
    }
}

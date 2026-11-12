using Api.Agente.Domain;

namespace Api.Agente.Application;

public sealed class AnalisadorDeterministico : IAnalisadorModelo
{
    public Task<SaidaModelo> AnalisarAsync(ContextoModelo contexto, CancellationToken ct)
    {
        // Vencedora definida: confirma. Empate (sem vencedora): escolhe o primeiro candidato.
        var escolhida = contexto.Vencedora
            ?? contexto.Evidencias.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.Ncm));
        return Task.FromResult(new SaidaModelo
        {
            Ncm = escolhida?.Ncm,
            Explicacao = escolhida?.Trecho
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

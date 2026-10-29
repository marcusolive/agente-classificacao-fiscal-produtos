using Api.Agente.Application;
using Api.Agente.Domain;

namespace Api.Agente.Tests.integration;

public sealed class DubleConsultaConhecimento : IConsultaConhecimento
{
    private readonly List<(string Texto, Evidencia Evidencia)> _itens = [];

    public void Registrar(string textoContem, Evidencia evidencia) => _itens.Add((textoContem, evidencia));

    public Task<IReadOnlyList<Evidencia>> ConsultarAsync(string texto, DateOnly vigencia, CancellationToken ct)
    {
        IReadOnlyList<Evidencia> encontradas = _itens
            .Where(i => texto.Contains(i.Texto, StringComparison.OrdinalIgnoreCase) && i.Evidencia.Vigencia == vigencia)
            .Select(i => i.Evidencia)
            .ToList();
        return Task.FromResult(encontradas);
    }
}

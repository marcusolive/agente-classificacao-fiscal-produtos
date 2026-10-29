using Api.Agente.Domain;

namespace Api.Agente.Application;

public interface IConsultaConhecimento
{
    Task<IReadOnlyList<Evidencia>> ConsultarAsync(string texto, DateOnly vigencia, CancellationToken ct);
}

public interface IRepositorioExecucao
{
    Task SalvarAsync(Execucao execucao, CancellationToken ct);
    Task<Execucao?> ObterAsync(Guid execucaoId, CancellationToken ct);
}

public sealed class ContextoModelo
{
    public required Evidencia Vencedora { get; init; }
    public required IReadOnlyList<Evidencia> Evidencias { get; init; }
}

public sealed class SaidaModelo
{
    public string? Ncm { get; set; }
    public string? Explicacao { get; set; }
}

public interface IAnalisadorModelo
{
    Task<SaidaModelo> AnalisarAsync(ContextoModelo contexto, CancellationToken ct);
}

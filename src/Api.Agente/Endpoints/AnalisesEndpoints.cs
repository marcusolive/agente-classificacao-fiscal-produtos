using Api.Agente.Application;

namespace Api.Agente.Endpoints;

public static class AnalisesEndpoints
{
    public static void Mapear(WebApplication app)
    {
        app.MapPost("/analises", async (PedidoAnalise pedido, AnalisarExecucao analisar, CancellationToken ct) =>
        {
            try
            {
                var execucao = await analisar.ExecutarAsync(pedido, ct);
                return Results.Ok(execucao);
            }
            catch (EntradaRejeitadaException ex)
            {
                return Results.BadRequest(new { erro = ex.Message });
            }
        });

        app.MapGet("/analises/{execucaoId:guid}", async (Guid execucaoId, AnalisarExecucao analisar, CancellationToken ct) =>
        {
            var execucao = await analisar.ObterAsync(execucaoId, ct);
            return execucao is null ? Results.NotFound() : Results.Ok(execucao);
        });
    }
}

using Api.Agente.Application;
using Api.Agente.Domain;

namespace Api.Agente.Endpoints;

public static class ConfirmacoesEndpoints
{
    public static void Mapear(WebApplication app)
    {
        app.MapGroup("/v1")
            .WithGroupName(ContratoApi.Documento)
            .WithTags("Confirmações v1")
            .MapPost("/analises/{execucaoId:guid}/confirmacoes", ConfirmarAsync)
            .WithName("ConfirmarCasoV1")
            .WithSummary("Confirma ou corrige itens de uma execução.")
            .WithDescription("Valida o papel informado e a decisão de cada item no contrato 1.1.0. Aceite usa a NCM sustentada. Correção só usa valor presente nas evidências. A resposta não grava caso corporativo e não altera a execução.")
            .Produces<ResultadoConfirmacao>(StatusCodes.Status200OK)
            .Produces<ErroResposta>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> ConfirmarAsync(
        Guid execucaoId,
        PedidoConfirmacao pedido,
        ConfirmadorDeCaso confirmador,
        CancellationToken ct)
    {
        try
        {
            var resultado = await confirmador.ConfirmarAsync(execucaoId, pedido, ct);
            return Results.Ok(resultado);
        }
        catch (ExecucaoNaoEncontradaException)
        {
            return Results.NotFound();
        }
        catch (EntradaRejeitadaException ex)
        {
            return Results.BadRequest(new ErroResposta { Erro = ex.Message });
        }
    }
}

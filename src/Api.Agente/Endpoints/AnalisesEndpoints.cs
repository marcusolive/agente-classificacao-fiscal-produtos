using Api.Agente.Application;
using Api.Agente.Domain;

namespace Api.Agente.Endpoints;

public static class AnalisesEndpoints
{
    public static void Mapear(WebApplication app)
    {
        var v1 = app.MapGroup("/v1")
            .WithGroupName(ContratoApi.Documento)
            .WithTags("Análises v1");

        v1.MapPost("/analises", CriarAsync)
            .WithName("CriarAnaliseV1")
            .WithSummary("Executa a análise dos itens.")
            .WithDescription("Recebe o contexto fiscal e os itens. Devolve sugestão, divergência ou bloqueio da nota no contrato 1.0.0. Contexto incompleto, tipo não aceito ou arquivo sem limite configurado responde 400 e não classifica.")
            .Produces<Execucao>(StatusCodes.Status200OK)
            .Produces<ErroResposta>(StatusCodes.Status400BadRequest);

        v1.MapGet("/analises/{execucaoId:guid}", ObterAsync)
            .WithName("ObterAnaliseV1")
            .WithSummary("Recupera uma análise já executada.")
            .WithDescription("Devolve o mesmo contrato 1.0.0 gravado na execução. O identificador é o execucaoId retornado na criação.")
            .Produces<Execucao>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CriarAsync(PedidoAnalise pedido, AnalisarExecucao analisar, CancellationToken ct)
    {
        try
        {
            var execucao = await analisar.ExecutarAsync(pedido, ct);
            return Results.Ok(execucao);
        }
        catch (EntradaRejeitadaException ex)
        {
            return Results.BadRequest(new ErroResposta { Erro = ex.Message, Classe = ex.Classe });
        }
    }

    private static async Task<IResult> ObterAsync(Guid execucaoId, AnalisarExecucao analisar, CancellationToken ct)
    {
        var execucao = await analisar.ObterAsync(execucaoId, ct);
        return execucao is null ? Results.NotFound() : Results.Ok(execucao);
    }
}

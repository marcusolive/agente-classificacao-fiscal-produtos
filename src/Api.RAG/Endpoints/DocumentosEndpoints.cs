using Api.RAG.Application;
using Api.RAG.Domain;

namespace Api.RAG.Endpoints;

public static class DocumentosEndpoints
{
    public static void Mapear(WebApplication app)
    {
        var v1 = app.MapGroup("/v1")
            .WithGroupName(ContratoApi.Documento)
            .WithTags("Documentos v1");

        v1.MapPost("/documentos", CriarAsync)
            .WithName("CriarDocumentoV1")
            .WithSummary("Ingere uma versão de documento.")
            .WithDescription("Exige a chave X-Chave-Ingestao. Grava texto, PDF ou XML de notas com origem, identificação e versão. Hash igual é ignorado. Conteúdo diferente na mesma versão é recusado. A consulta da análise não é esta rota: ela é a ferramenta MCP consultar_conhecimento.")
            .Produces<ResultadoIngestao>(StatusCodes.Status200OK)
            .Produces<ErroResposta>(StatusCodes.Status400BadRequest);

        v1.MapGet("/documentos/{documentoId:guid}", ObterAsync)
            .WithName("ObterDocumentoV1")
            .WithSummary("Lê uma versão gravada.")
            .WithDescription("Devolve o documento e os chunks dessa versão, inclusive o caso confirmado.")
            .Produces<Documento>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        v1.MapGet("/documentos", ListarAsync)
            .WithName("ListarDocumentosV1")
            .WithSummary("Lista as versões de um documento.")
            .WithDescription("Filtra por origem e identificação. Cada versão permanece separada.")
            .Produces<IReadOnlyList<Documento>>(StatusCodes.Status200OK)
            .Produces<ErroResposta>(StatusCodes.Status400BadRequest);

        v1.MapDelete("/documentos", RemoverAsync)
            .WithName("RemoverDocumentosV1")
            .WithSummary("Remove todas as versões de uma origem.")
            .WithDescription("Exige a chave X-Chave-Ingestao. Apaga documentos e chunks da origem informada, para recarga de uma base de referência (ex.: a tabela NCM). Operação administrativa e destrutiva, limitada a uma origem.")
            .Produces(StatusCodes.Status200OK)
            .Produces<ErroResposta>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> CriarAsync(
        PedidoDocumento pedido,
        IngestaoDocumento ingestao,
        HttpRequest requisicao,
        CancellationToken ct)
    {
        try
        {
            var resultado = await ingestao.IngerirAsync(pedido, requisicao.Headers["X-Chave-Ingestao"], ct);
            return Results.Ok(resultado);
        }
        catch (RecusaIngestaoException ex)
        {
            return Results.BadRequest(new ErroResposta { Erro = ex.Message });
        }
    }

    private static async Task<IResult> ObterAsync(Guid documentoId, IngestaoDocumento ingestao, CancellationToken ct)
    {
        var documento = await ingestao.ObterAsync(documentoId, ct);
        return documento is null ? Results.NotFound() : Results.Ok(documento);
    }

    private static async Task<IResult> ListarAsync(
        string? origem,
        string? identificacao,
        IngestaoDocumento ingestao,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(origem) || string.IsNullOrWhiteSpace(identificacao))
        {
            return Results.BadRequest(new ErroResposta { Erro = "origem e identificacao são obrigatórias." });
        }

        return Results.Ok(await ingestao.ListarAsync(origem, identificacao, ct));
    }

    private static async Task<IResult> RemoverAsync(
        string? origem,
        IngestaoDocumento ingestao,
        HttpRequest requisicao,
        CancellationToken ct)
    {
        try
        {
            var removidos = await ingestao.RemoverPorOrigemAsync(origem ?? "", requisicao.Headers["X-Chave-Ingestao"], ct);
            return Results.Ok(new { origem = (origem ?? "").Trim(), removidos });
        }
        catch (RecusaIngestaoException ex)
        {
            return Results.BadRequest(new ErroResposta { Erro = ex.Message });
        }
    }
}

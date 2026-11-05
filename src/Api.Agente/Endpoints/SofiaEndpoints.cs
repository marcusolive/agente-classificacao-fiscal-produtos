using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Agente.Application;
using Api.Agente.Domain;
using Microsoft.Extensions.Options;

namespace Api.Agente.Endpoints;

public static class SofiaEndpoints
{
    public const string PoliticaCors = "SofiaBrowser";
    private static readonly JsonSerializerOptions Json = CriarJson();

    public static void Mapear(WebApplication app, bool cors)
    {
        var card = app.MapGet("/.well-known/agent-card.json", Cartao)
            .WithGroupName(ContratoApi.Documento)
            .WithTags("SOFIA A2A")
            .WithName("ObterAgentCardV1")
            .WithSummary("Publica o Agent Card.")
            .WithDescription("Descoberta A2A v0.3. A propriedade url é o endpoint JSON-RPC. Esta rota não substitui POST /v1/analises.")
            .Produces<CartaoAgente>(StatusCodes.Status200OK);

        var envio = app.MapPost("/a2a", EnviarAsync)
            .WithGroupName(ContratoApi.Documento)
            .WithTags("SOFIA A2A")
            .WithName("EnviarMensagemA2AV1")
            .WithSummary("Recebe message/send.")
            .WithDescription("JSON-RPC 2.0. O texto da mensagem é SolicitacaoAnaliseSofia 1.0. A tarefa concluída devolve ResultadoAnaliseSofia em um TextPart. Exige Bearer. Erro de protocolo responde HTTP 200 com error.")
            .Produces<RespostaRpc>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        if (!cors)
        {
            return;
        }

        card.RequireCors(PoliticaCors);
        envio.RequireCors(PoliticaCors);
    }

    private static IResult Cartao(HttpRequest requisicao, IOptions<OpcoesSofia> opcoes)
    {
        var sofia = opcoes.Value;
        var url = string.IsNullOrWhiteSpace(sofia.Url)
            ? $"{requisicao.Scheme}://{requisicao.Host}/a2a"
            : sofia.Url.Trim();
        return Results.Json(new CartaoAgente
        {
            Name = string.IsNullOrWhiteSpace(sofia.Nome) ? "Agente Fiscal INFIS" : sofia.Nome.Trim(),
            Description = "Analisa documentos fiscais enviados pela SOFIA",
            Url = url
        }, Json);
    }

    private static async Task<IResult> EnviarAsync(
        HttpRequest requisicao,
        AtendenteSofia atendente,
        IOptions<OpcoesSofia> opcoes,
        CancellationToken ct)
    {
        if (!Autorizado(requisicao, opcoes.Value))
        {
            return Results.Unauthorized();
        }

        JsonElement corpo;
        try
        {
            corpo = await JsonSerializer.DeserializeAsync<JsonElement>(requisicao.Body, Json, ct);
        }
        catch (JsonException)
        {
            return Rpc(null, ErroRpc.Parse);
        }

        if (!CorpoValido(corpo, out var id, out var erro))
        {
            return Rpc(id, erro!);
        }

        var mensagem = corpo.GetProperty("params").GetProperty("message");
        if (!ParteTexto(mensagem, out var texto, out erro))
        {
            return Rpc(id, erro!);
        }

        SolicitacaoAnaliseSofia? pedido;
        try
        {
            pedido = JsonSerializer.Deserialize<SolicitacaoAnaliseSofia>(texto, Json);
        }
        catch (JsonException)
        {
            return Rpc(id, ErroRpc.Parametros("O texto da mensagem não é o contrato SOFIA 1.0."));
        }

        try
        {
            var (tarefaId, resultado) = await atendente.AtenderAsync(pedido, ct);
            return Rpc(id, null, Tarefa(tarefaId, resultado));
        }
        catch (SolicitacaoInvalidaException ex)
        {
            return Rpc(id, ErroRpc.Parametros(ex.Message));
        }
    }

    private static bool Autorizado(HttpRequest requisicao, OpcoesSofia sofia)
    {
        if (string.IsNullOrEmpty(sofia.Chave))
        {
            return false;
        }

        var cabecalho = requisicao.Headers.Authorization.ToString();
        const string prefixo = "Bearer ";
        return cabecalho.StartsWith(prefixo, StringComparison.Ordinal)
            && cabecalho[prefixo.Length..] == sofia.Chave;
    }

    private static bool CorpoValido(JsonElement corpo, out JsonElement? id, out ErroRpc? erro)
    {
        id = corpo.TryGetProperty("id", out var encontrado) ? encontrado.Clone() : null;
        erro = null;
        if (corpo.ValueKind != JsonValueKind.Object
            || !corpo.TryGetProperty("jsonrpc", out var versao)
            || versao.GetString() != "2.0"
            || id is null)
        {
            erro = ErroRpc.Requisicao;
            return false;
        }

        if (!corpo.TryGetProperty("method", out var metodo) || metodo.GetString() != "message/send")
        {
            erro = ErroRpc.Metodo;
            return false;
        }

        if (!corpo.TryGetProperty("params", out var parametros)
            || !parametros.TryGetProperty("message", out var mensagem)
            || !mensagem.TryGetProperty("role", out var papel)
            || papel.GetString() != "user"
            || !mensagem.TryGetProperty("messageId", out var messageId)
            || string.IsNullOrWhiteSpace(messageId.GetString()))
        {
            erro = ErroRpc.Parametros("message/send exige role user e messageId.");
            return false;
        }

        return true;
    }

    private static bool ParteTexto(JsonElement mensagem, out string texto, out ErroRpc? erro)
    {
        texto = "";
        erro = null;
        if (!mensagem.TryGetProperty("parts", out var partes) || partes.ValueKind != JsonValueKind.Array)
        {
            erro = ErroRpc.Parametros("A mensagem exige um TextPart.");
            return false;
        }

        var textos = partes.EnumerateArray()
            .Where(p => p.TryGetProperty("kind", out var kind) && kind.GetString() == "text")
            .Select(p => p.TryGetProperty("text", out var valor) ? valor.GetString() : null)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();
        if (textos.Count != 1)
        {
            erro = ErroRpc.Parametros("A mensagem exige um TextPart.");
            return false;
        }

        texto = textos[0]!;
        return true;
    }

    private static TarefaA2A Tarefa(string tarefaId, ResultadoAnaliseSofia resultado) => new()
    {
        Id = tarefaId,
        ContextId = resultado.IdExecucao,
        Status = new EstadoTarefa("completed"),
        Artifacts =
        [
            new ArtefatoA2A
            {
                ArtifactId = $"art-{tarefaId}",
                Name = "analise-sofia-v1",
                Parts = [new ParteTextoA2A(JsonSerializer.Serialize(resultado, Json))]
            }
        ]
    };

    private static IResult Rpc(JsonElement? id, ErroRpc? erro, TarefaA2A? tarefa = null) =>
        Results.Json(new RespostaRpc { Id = id, Result = tarefa, Error = erro }, Json);

    private static JsonSerializerOptions CriarJson()
    {
        var opcoes = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        opcoes.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return opcoes;
    }
}

public sealed class CartaoAgente
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Url { get; init; }
    public string Version { get; init; } = "1.0.0";
    public CapacidadesAgente Capabilities { get; init; } = new(false, false);
    public string[] DefaultInputModes { get; init; } = ["text"];
    public string[] DefaultOutputModes { get; init; } = ["text"];
    public HabilidadeAgente[] Skills { get; init; } = [new()];
}

public sealed record CapacidadesAgente(bool Streaming, bool PushNotifications);

public sealed class HabilidadeAgente
{
    public string Id { get; init; } = "analisar_documentos";
    public string Name { get; init; } = "Análise documental";
    public string Description { get; init; } = "Analisa textos enviados pela SOFIA";
    public string[] Tags { get; init; } = ["infis", "fiscal", "sofia"];
}

public sealed class RespostaRpc
{
    public string Jsonrpc { get; init; } = "2.0";
    public JsonElement? Id { get; init; }
    public TarefaA2A? Result { get; init; }
    public ErroRpc? Error { get; init; }
}

public sealed class TarefaA2A
{
    public required string Id { get; init; }
    public required string ContextId { get; init; }
    public required EstadoTarefa Status { get; init; }
    public required List<ArtefatoA2A> Artifacts { get; init; }
}

public sealed record EstadoTarefa(string State);

public sealed class ArtefatoA2A
{
    public required string ArtifactId { get; init; }
    public required string Name { get; init; }
    public required List<ParteTextoA2A> Parts { get; init; }
}

public sealed record ParteTextoA2A(string Text)
{
    public string Kind { get; init; } = "text";
}

public sealed record ErroRpc(int Code, string Message)
{
    public static readonly ErroRpc Parse = new(-32700, "JSON inválido.");
    public static readonly ErroRpc Requisicao = new(-32600, "Requisição JSON-RPC inválida.");
    public static readonly ErroRpc Metodo = new(-32601, "Método não encontrado.");
    public static ErroRpc Parametros(string mensagem) => new(-32602, mensagem);
}

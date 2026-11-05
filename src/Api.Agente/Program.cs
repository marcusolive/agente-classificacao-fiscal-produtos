using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Agente.Application;
using Api.Agente.Domain;
using Api.Agente.Endpoints;
using Api.Agente.Infrastructure;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<OpcoesAnalise>(builder.Configuration.GetSection("Analise"));
builder.Services.Configure<OpcoesOpenAi>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<OpcoesConhecimento>(builder.Configuration.GetSection("Conhecimento"));
builder.Services.Configure<OpcoesSofia>(builder.Configuration.GetSection("Sofia"));
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

var testando = builder.Environment.IsEnvironment("Testing");
var openAi = builder.Configuration.GetSection("OpenAI").Get<OpcoesOpenAi>() ?? new OpcoesOpenAi();
var conexao = builder.Configuration.GetConnectionString("Agente");

if (!testando && !string.IsNullOrWhiteSpace(openAi.ApiKey) && !string.IsNullOrWhiteSpace(openAi.Model))
{
    builder.Services.AddSingleton<IAnalisadorModelo, OpenAiAnalisador>();
}
else
{
    builder.Services.AddSingleton<IAnalisadorModelo, AnalisadorDeterministico>();
}

builder.Services.AddSingleton<IConsultaConhecimento, McpConsultaConhecimento>();
if (!testando && !string.IsNullOrWhiteSpace(conexao))
{
    builder.Services.AddSingleton<IRepositorioExecucao>(_ => new PostgresRepositorioExecucao(conexao));
}
else
{
    builder.Services.AddSingleton<IRepositorioExecucao, RepositorioExecucaoMemoria>();
}

builder.Services.AddSingleton<RegistroDeLayouts>();
builder.Services.AddSingleton<ValidadorEntrada>();
builder.Services.AddSingleton<ClassificadorDeItem>();
builder.Services.AddSingleton<AnalisarExecucao>();
builder.Services.AddSingleton<ConfirmadorDeCaso>();
builder.Services.AddSingleton<ValidadorSolicitacaoSofia>();
builder.Services.AddSingleton<MemoriaSolicitacaoSofia>();
builder.Services.AddSingleton<AtendenteSofia>();
var origensCors = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?
    .Where(origem => !string.IsNullOrWhiteSpace(origem))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray() ?? [];
if (origensCors.Length > 0)
{
    builder.Services.AddCors(o => o.AddPolicy(SofiaEndpoints.PoliticaCors, politica =>
        politica.WithOrigins(origensCors)
            .WithMethods("GET", "POST", "OPTIONS")
            .WithHeaders("Content-Type", "Authorization")));
}
builder.Services.AddOpenApi(ContratoApi.Documento, options =>
{
    options.AddDocumentTransformer((documento, _, _) =>
    {
        documento.Info ??= new OpenApiInfo();
        documento.Info.Title = ContratoApi.Titulo;
        documento.Info.Version = ContratoApi.VersaoDocumento;
        documento.Info.Description = ContratoApi.Descricao;
        documento.Servers = [new OpenApiServer { Url = "/" }];
        return Task.CompletedTask;
    });
    options.AddOperationTransformer((operacao, _, _) =>
    {
        if (operacao.Responses is not null)
        {
            foreach (var resposta in operacao.Responses)
            {
                if (resposta.Value is not OpenApiResponse corpo)
                {
                    continue;
                }

                corpo.Description = (operacao.OperationId, resposta.Key) switch
                {
                    ("CriarAnaliseV1", "200") => "Análise processada. O estado da nota pode ser bloqueio, pendência, falha ou sugestão liberada.",
                    ("CriarAnaliseV1", "400") => "Contexto ausente, tipo não aceito, arquivo vazio, arquivo sem limite ou layout não registrado. Não há bloqueio fiscal.",
                    ("ObterAnaliseV1", "200") => "Execução encontrada no contrato 1.0.0.",
                    ("ObterAnaliseV1", "404") => "Execução não encontrada.",
                    ("ConfirmarCasoV1", "200") => "Confirmação validada no contrato 1.1.0. O caso não foi gravado.",
                    ("ConfirmarCasoV1", "400") => "Papel ausente, item inexistente ou código que não está nas evidências da execução.",
                    ("ConfirmarCasoV1", "404") => "Execução não encontrada.",
                    ("ObterAgentCardV1", "200") => "Agent Card A2A. A propriedade url é o endpoint message/send.",
                    ("EnviarMensagemA2AV1", "200") => "JSON-RPC message/send. O artefato contém ResultadoAnaliseSofia. Erro de protocolo usa o campo error.",
                    ("EnviarMensagemA2AV1", "401") => "Bearer ausente ou diferente da chave configurada.",
                    _ => corpo.Description
                };
            }
        }

        if (operacao.Parameters is null)
        {
            return Task.CompletedTask;
        }

        foreach (var parametro in operacao.Parameters)
        {
            if (parametro is OpenApiParameter caminho && caminho.Name == "execucaoId")
            {
                caminho.Description = "Identificador devolvido na criação da análise.";
            }
        }

        return Task.CompletedTask;
    });
    options.AddSchemaTransformer(DocumentacaoDeSchema.Transformar);
});

var app = builder.Build();
if (!testando && !string.IsNullOrWhiteSpace(conexao))
{
    var postgres = (PostgresRepositorioExecucao)app.Services.GetRequiredService<IRepositorioExecucao>();
    await postgres.GarantirEsquemaAsync(CancellationToken.None);
}

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapOpenApi();
app.MapSwaggerUI("/swagger", options =>
{
    options.SwaggerEndpoint($"/openapi/{ContratoApi.Documento}.json", $"{ContratoApi.Titulo} {ContratoApi.VersaoDocumento}");
    options.DocumentTitle = $"{ContratoApi.Titulo} {ContratoApi.VersaoDocumento}";
});
if (origensCors.Length > 0)
{
    app.UseCors();
}

AnalisesEndpoints.Mapear(app);
ConfirmacoesEndpoints.Mapear(app);
SofiaEndpoints.Mapear(app, origensCors.Length > 0);
app.Run();

public partial class Program;

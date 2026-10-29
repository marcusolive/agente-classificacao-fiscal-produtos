using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Agente.Application;
using Api.Agente.Domain;
using Api.Agente.Endpoints;
using Api.Agente.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<OpcoesAnalise>(builder.Configuration.GetSection("Analise"));
builder.Services.Configure<OpcoesOpenAi>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<OpcoesConhecimento>(builder.Configuration.GetSection("Conhecimento"));
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

builder.Services.AddSingleton<ValidadorEntrada>();
builder.Services.AddSingleton<ClassificadorDeItem>();
builder.Services.AddSingleton<AnalisarExecucao>();

var app = builder.Build();
if (!testando && !string.IsNullOrWhiteSpace(conexao))
{
    var postgres = (PostgresRepositorioExecucao)app.Services.GetRequiredService<IRepositorioExecucao>();
    await postgres.GarantirEsquemaAsync(CancellationToken.None);
}

AnalisesEndpoints.Mapear(app);
app.Run();

public partial class Program;

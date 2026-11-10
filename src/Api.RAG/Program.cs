using System.Text.Json;
using System.Text.Json.Serialization;
using Api.RAG.Application;
using Api.RAG.Domain;
using Api.RAG.Endpoints;
using Api.RAG.Infrastructure;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<OpcoesIngestao>(builder.Configuration.GetSection("Ingestao"));
builder.Services.Configure<OpcoesEmbedding>(builder.Configuration.GetSection("Embedding"));
builder.Services.Configure<OpcoesOpenAi>(builder.Configuration.GetSection("OpenAI"));
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

var testando = builder.Environment.IsEnvironment("Testing");
var conexao = builder.Configuration.GetConnectionString("Rag");
var dimensao = builder.Configuration.GetValue<int?>("Embedding:Dimensao");
if (!testando && !string.IsNullOrWhiteSpace(conexao))
{
    builder.Services.AddSingleton<IRepositorioDocumento>(_ => new PostgresRepositorioDocumento(conexao, dimensao));
}
else
{
    builder.Services.AddSingleton<IRepositorioDocumento, RepositorioDocumentoMemoria>();
}

builder.Services.AddSingleton<ValidadorIngestao>();
builder.Services.AddSingleton<ExtratorTexto>();
builder.Services.AddSingleton<Segmentador>();
builder.Services.AddHttpClient(ProvedorEmbeddingOpenAi.Cliente);
builder.Services.AddSingleton<IProvedorEmbedding, ProvedorEmbeddingOpenAi>();
builder.Services.AddSingleton<IngestaoDocumento>();
builder.Services.AddSingleton<ConsultaConhecimento>();
builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();
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
                    ("CriarDocumentoV1", "200") => "Documento gravado ou ignorado por hash igual. A versão só fica consultável quando a busca vetorial existir.",
                    ("CriarDocumentoV1", "400") => "Chave ausente, campo obrigatório ausente, formato não aceito, arquivo sem limite ou conteúdo diferente na mesma versão.",
                    ("ObterDocumentoV1", "200") => "Documento encontrado, com os chunks da versão.",
                    ("ObterDocumentoV1", "404") => "Documento não encontrado.",
                    ("ListarDocumentosV1", "200") => "Versões da mesma origem e identificação, sem misturar o texto delas.",
                    ("ListarDocumentosV1", "400") => "origem e identificacao são obrigatórias.",
                    _ => corpo.Description
                };
            }
        }

        operacao.Parameters ??= [];
        if (operacao.OperationId == "CriarDocumentoV1"
            && !operacao.Parameters.OfType<OpenApiParameter>().Any(p => p.Name == "X-Chave-Ingestao"))
        {
            operacao.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Chave-Ingestao",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Chave de ingestão configurada no ambiente. Chave vazia recusa o pedido."
            });
        }

        foreach (var parametro in operacao.Parameters.OfType<OpenApiParameter>())
        {
            parametro.Description = parametro.Name switch
            {
                "documentoId" => "Identificador devolvido na ingestão.",
                "origem" => "Origem do documento.",
                "identificacao" => "Identificação do documento dentro da origem.",
                "X-Chave-Ingestao" => parametro.Description,
                _ => parametro.Description
            };
            if (parametro.Name is "origem" or "identificacao")
            {
                parametro.Required = true;
            }
        }

        return Task.CompletedTask;
    });
    options.AddSchemaTransformer(DocumentacaoDeSchema.Transformar);
});

var app = builder.Build();
if (!testando && !string.IsNullOrWhiteSpace(conexao))
{
    var postgres = (PostgresRepositorioDocumento)app.Services.GetRequiredService<IRepositorioDocumento>();
    await postgres.GarantirEsquemaAsync(CancellationToken.None);
}

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapOpenApi();
app.MapSwaggerUI("/swagger", options =>
{
    options.SwaggerEndpoint($"/openapi/{ContratoApi.Documento}.json", $"{ContratoApi.Titulo} {ContratoApi.VersaoDocumento}");
    options.DocumentTitle = $"{ContratoApi.Titulo} {ContratoApi.VersaoDocumento}";
});
DocumentosEndpoints.Mapear(app);
app.MapMcp("/mcp");
NavegadorLocal.AgendarSwagger(app);
app.Run();

public partial class Program;

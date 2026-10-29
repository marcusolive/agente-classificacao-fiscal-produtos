using Api.RAG.Application;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<OpcoesIngestao>(builder.Configuration.GetSection("Ingestao"));
builder.Services.Configure<OpcoesEmbedding>(builder.Configuration.GetSection("Embedding"));

var app = builder.Build();
app.Run();

public partial class Program;

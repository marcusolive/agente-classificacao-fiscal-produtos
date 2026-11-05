using Api.RAG.Application;
using Api.RAG.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Api.RAG.Tests.integration;

public sealed class FabricaApi : WebApplicationFactory<Program>
{
    public bool BuscaConfigurada { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Ingestao:Chave", "segredo-teste");
        if (!BuscaConfigurada)
        {
            return;
        }

        builder.UseSetting("Embedding:Model", "teste");
        builder.UseSetting("Embedding:Dimensao", "3");
        builder.UseSetting("Embedding:TopK", "5");
        builder.UseSetting("Embedding:Limiar", "0.5");
        builder.ConfigureServices(services =>
        {
            foreach (var descriptor in services.Where(d => d.ServiceType == typeof(IProvedorEmbedding)).ToList())
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<IProvedorEmbedding, DubleEmbedding>();
        });
    }
}

public sealed class DubleEmbedding : IProvedorEmbedding
{
    public Task IncorporarAsync(Documento documento, CancellationToken ct)
    {
        documento.Consultavel = true;
        return Task.CompletedTask;
    }
}

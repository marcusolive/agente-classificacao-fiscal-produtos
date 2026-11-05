using System.Globalization;
using Api.RAG.Application;
using Api.RAG.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Api.RAG.Tests.integration;

public sealed class FabricaApi : WebApplicationFactory<Program>
{
    public bool BuscaConfigurada { get; init; }

    public string TopK { get; init; } = "5";

    public string Limiar { get; init; } = "0.5";

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
        builder.UseSetting("Embedding:TopK", TopK);
        builder.UseSetting("Embedding:Limiar", Limiar);
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
        foreach (var chunk in documento.Chunks)
        {
            chunk.Embedding = Vetor(chunk.Texto);
        }

        return Task.CompletedTask;
    }

    public Task<float[]> IncorporarConsultaAsync(string texto, CancellationToken ct) =>
        Task.FromResult(Vetor(texto));

    public static float[] Vetor(string texto)
    {
        if (texto.StartsWith("vec:", StringComparison.Ordinal))
        {
            return texto[4..]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(parte => float.Parse(parte, CultureInfo.InvariantCulture))
                .ToArray();
        }

        return [1f, 0f, 0f];
    }
}

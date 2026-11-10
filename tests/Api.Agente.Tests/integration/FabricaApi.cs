using Api.Agente.Application;
using Api.Agente.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Agente.Tests.integration;

public sealed class FabricaApi : WebApplicationFactory<Program>
{
    public DubleConsultaConhecimento Conhecimento { get; } = new();

    public string? OrigemCors { get; init; }

    public string? SofiaToken { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        // Quando SofiaToken está definido, Sofia:Chave fica vazia para provar que
        // SOFIA_A2A_TOKEN é a fonte efetiva do segredo de entrada A2A.
        builder.UseSetting("Sofia:Chave", string.IsNullOrWhiteSpace(SofiaToken) ? "chave-sofia-teste" : "");
        if (!string.IsNullOrWhiteSpace(SofiaToken))
        {
            builder.UseSetting("SOFIA_A2A_TOKEN", SofiaToken);
        }
        if (!string.IsNullOrWhiteSpace(OrigemCors))
        {
            builder.UseSetting("Cors:AllowedOrigins:0", OrigemCors);
        }
        builder.ConfigureServices(services =>
        {
            Trocar<IConsultaConhecimento>(services, Conhecimento);
            Trocar<IAnalisadorModelo>(services, new AnalisadorDeterministico());
            Trocar<IRepositorioExecucao>(services, new RepositorioExecucaoMemoria());
        });
    }

    private static void Trocar<T>(IServiceCollection services, T instancia) where T : class
    {
        foreach (var descriptor in services.Where(d => d.ServiceType == typeof(T)).ToList())
        {
            services.Remove(descriptor);
        }

        services.AddSingleton(instancia);
    }
}

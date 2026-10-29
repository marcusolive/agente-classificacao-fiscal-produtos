using Api.Agente.Application;
using Api.Agente.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Agente.Tests.integration;

public sealed class FabricaApi : WebApplicationFactory<Program>
{
    public DubleConsultaConhecimento Conhecimento { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
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

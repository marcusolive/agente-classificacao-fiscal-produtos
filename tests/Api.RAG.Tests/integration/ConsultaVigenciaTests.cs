using System.Text.Json;
using Api.RAG.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Api.RAG.Tests.integration;

public class ConsultaVigenciaTests : IDisposable
{
    private readonly FabricaApi _fabrica = new() { BuscaConfigurada = true };
    private readonly FabricaApi _semBusca = new();

    [Fact]
    public async Task Data_devolve_so_a_versao_daquele_intervalo()
    {
        var cliente = ClienteDocumento.Criar(_fabrica);
        await ClienteDocumento.PostarAsync(cliente, ClienteDocumento.Texto("vigencia de janeiro", "1", "2026-01-01", "2026-03-31"));
        await ClienteDocumento.PostarAsync(cliente, ClienteDocumento.Texto("vigencia de junho", "2", "2026-06-01", "2026-08-31"));
        var consulta = _fabrica.Services.GetRequiredService<ConsultaConhecimento>();

        var janeiro = await consulta.ConsultarAsync("produto", new DateOnly(2026, 2, 1), CancellationToken.None);
        var vazio = await consulta.ConsultarAsync("produto", new DateOnly(2026, 12, 1), CancellationToken.None);

        Assert.Equal(["oficial/ncm/1"], janeiro.Select(e => e.Fonte).Distinct().ToArray());
        Assert.Equal(new DateOnly(2026, 2, 1), janeiro[0].Vigencia);
        Assert.Empty(vazio);
    }

    [Fact]
    public async Task Busca_sem_parametro_responde_erro()
    {
        var consulta = _semBusca.Services.GetRequiredService<ConsultaConhecimento>();
        var erro = await Assert.ThrowsAsync<BuscaIndisponivelException>(() =>
            consulta.ConsultarAsync("produto", new DateOnly(2026, 1, 1), CancellationToken.None));
        Assert.Equal("A busca vetorial não está configurada.", erro.Message);
    }

    public void Dispose()
    {
        _fabrica.Dispose();
        _semBusca.Dispose();
    }
}

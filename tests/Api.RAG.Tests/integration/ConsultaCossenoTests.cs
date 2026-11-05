using Api.RAG.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Api.RAG.Tests.integration;

public class ConsultaCossenoTests : IDisposable
{
    private readonly List<IDisposable> _fabricas = [];

    [Fact]
    public async Task TopK_devolve_o_chunk_mais_proximo()
    {
        var fabrica = Nova(topK: "1", limiar: "0.5");
        var cliente = ClienteDocumento.Criar(fabrica);
        await ClienteDocumento.PostarAsync(cliente, ClienteDocumento.Texto("vec:1,0,0", "c1"));
        await ClienteDocumento.PostarAsync(cliente, ClienteDocumento.Texto("vec:0.6,0.8,0", "c2"));
        var consulta = fabrica.Services.GetRequiredService<ConsultaConhecimento>();

        var evidencias = await consulta.ConsultarAsync("vec:1,0,0", new DateOnly(2026, 1, 15), CancellationToken.None);

        Assert.Equal(["oficial/ncm/c1"], evidencias.Select(e => e.Fonte).ToArray());
    }

    [Fact]
    public async Task Abaixo_do_limiar_a_busca_termina_vazia()
    {
        var fabrica = Nova(topK: "5", limiar: "0.9");
        var cliente = ClienteDocumento.Criar(fabrica);
        await ClienteDocumento.PostarAsync(cliente, ClienteDocumento.Texto("vec:0,1,0", "c3"));
        var consulta = fabrica.Services.GetRequiredService<ConsultaConhecimento>();

        var evidencias = await consulta.ConsultarAsync("vec:1,0,0", new DateOnly(2026, 1, 15), CancellationToken.None);

        Assert.Empty(evidencias);
    }

    [Fact]
    public async Task Dimensao_diferente_da_configuracao_e_erro()
    {
        var fabrica = Nova(topK: "5", limiar: "0.5");
        using var derivada = fabrica.WithWebHostBuilder(b => b.UseSetting("Embedding:Dimensao", "4"));
        var consulta = derivada.Services.GetRequiredService<ConsultaConhecimento>();

        var erro = await Assert.ThrowsAsync<BuscaIndisponivelException>(() =>
            consulta.ConsultarAsync("produto", new DateOnly(2026, 1, 15), CancellationToken.None));

        Assert.Equal("A dimensão do embedding não confere.", erro.Message);
    }

    public void Dispose()
    {
        foreach (var fabrica in _fabricas)
        {
            fabrica.Dispose();
        }
    }

    private FabricaApi Nova(string topK, string limiar)
    {
        var fabrica = new FabricaApi { BuscaConfigurada = true, TopK = topK, Limiar = limiar };
        _fabricas.Add(fabrica);
        return fabrica;
    }
}

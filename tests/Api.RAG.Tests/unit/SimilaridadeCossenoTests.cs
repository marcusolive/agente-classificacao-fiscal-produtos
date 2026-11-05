using Api.RAG.Application;

namespace Api.RAG.Tests.unit;

public class SimilaridadeCossenoTests
{
    [Fact]
    public void Vetores_iguais_tem_similaridade_um_e_ortogonais_tem_zero()
    {
        Assert.Equal(1, SimilaridadeCosseno.Calcular([1f, 0f, 0f], [1f, 0f, 0f]), 6);
        Assert.Equal(0, SimilaridadeCosseno.Calcular([1f, 0f, 0f], [0f, 1f, 0f]), 6);
        Assert.Equal(0, SimilaridadeCosseno.Calcular([0f, 0f], [1f, 0f]), 6);
    }

    [Fact]
    public void Limiar_fora_de_zero_a_um_nao_configura_a_busca()
    {
        Assert.False(Configurada(null));
        Assert.False(Configurada(1.1));
        Assert.False(Configurada(-0.01));
        Assert.True(Configurada(0.5));
    }

    private static bool Configurada(double? limiar) =>
        IngestaoDocumento.BuscaConfigurada(new OpcoesEmbedding
        {
            Model = "teste",
            Dimensao = 3,
            TopK = 5,
            Limiar = limiar
        });
}

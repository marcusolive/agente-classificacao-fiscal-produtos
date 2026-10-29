using Api.Agente.Domain;

namespace Api.Agente.Tests.unit;

public class ConfiancaPorFaixaTests
{
    [Theory]
    [InlineData(FaixaAutoridade.LegislacaoVigente, 90)]
    [InlineData(FaixaAutoridade.NotasRgi, 80)]
    [InlineData(FaixaAutoridade.CasoCorporativoConfirmado, 65)]
    [InlineData(FaixaAutoridade.TextoFornecedor, 40)]
    public void Calcula_a_nota_da_faixa(FaixaAutoridade faixa, int esperada)
    {
        Assert.Equal(esperada, ConfiancaPorFaixa.Calcular(faixa, empate: false, possuiTrecho: true));
    }

    [Fact]
    public void Empate_ou_trecho_ausente_nao_viram_zero()
    {
        Assert.Null(ConfiancaPorFaixa.Calcular(FaixaAutoridade.LegislacaoVigente, empate: true, possuiTrecho: true));
        Assert.Null(ConfiancaPorFaixa.Calcular(FaixaAutoridade.LegislacaoVigente, empate: false, possuiTrecho: false));
        Assert.True(ConfiancaPorFaixa.ExigeRevisao(null));
        Assert.True(ConfiancaPorFaixa.ExigeRevisao(69));
        Assert.False(ConfiancaPorFaixa.ExigeRevisao(70));
    }
}

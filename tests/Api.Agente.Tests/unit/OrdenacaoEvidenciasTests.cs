using Api.Agente.Domain;

namespace Api.Agente.Tests.unit;

public class OrdenacaoEvidenciasTests
{
    private static Evidencia Nova(FaixaAutoridade faixa, string ncm) => new()
    {
        Fonte = faixa.ToString(),
        FaixaAutoridade = faixa,
        Trecho = "trecho",
        Vigencia = new DateOnly(2026, 1, 1),
        Ncm = ncm
    };

    [Fact]
    public void Legislacao_vence_o_texto_do_fornecedor()
    {
        var resultado = OrdenacaoDeEvidencias.Ordenar(
        [
            Nova(FaixaAutoridade.TextoFornecedor, "7326.90.90"),
            Nova(FaixaAutoridade.LegislacaoVigente, "7318.15.00")
        ]);

        Assert.False(resultado.Empate);
        Assert.Equal("7318.15.00", resultado.Vencedora?.Ncm);
        Assert.Equal(FaixaAutoridade.LegislacaoVigente, resultado.Ordenadas[0].FaixaAutoridade);
    }

    [Fact]
    public void Duas_legislacoes_diferentes_empatam()
    {
        var resultado = OrdenacaoDeEvidencias.Ordenar(
        [
            Nova(FaixaAutoridade.LegislacaoVigente, "7318.15.00"),
            Nova(FaixaAutoridade.LegislacaoVigente, "7318.16.00")
        ]);

        Assert.True(resultado.Empate);
        Assert.Null(resultado.Vencedora);
    }
}

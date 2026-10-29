using Api.Agente.Domain;

namespace Api.Agente.Tests.unit;

public class EstadoNotaTests
{
    private static ItemAnalise Item(SituacaoItem situacao) => new()
    {
        ItemId = situacao.ToString(),
        TextoOriginal = "item",
        Situacao = situacao
    };

    [Fact]
    public void Divergencia_bloqueia_mesmo_com_falha()
    {
        var estado = CalculadoraEstadoNota.Calcular([Item(SituacaoItem.FalhaTecnica), Item(SituacaoItem.Divergente)]);
        Assert.Equal(EstadoNota.Bloqueada, estado);
    }

    [Fact]
    public void Dado_ausente_fica_pendente_sem_divergencia()
    {
        var estado = CalculadoraEstadoNota.Calcular([Item(SituacaoItem.Sugerido), Item(SituacaoItem.DadoAusente)]);
        Assert.Equal(EstadoNota.PendenteDeDado, estado);
    }

    [Fact]
    public void Falha_tecnica_nao_libera_a_sugestao()
    {
        var estado = CalculadoraEstadoNota.Calcular([Item(SituacaoItem.Sugerido), Item(SituacaoItem.FalhaTecnica)]);
        Assert.Equal(EstadoNota.FalhaTecnica, estado);
    }

    [Fact]
    public void Itens_sugeridos_liberam_a_nota()
    {
        var estado = CalculadoraEstadoNota.Calcular([Item(SituacaoItem.Sugerido)]);
        Assert.Equal(EstadoNota.SugestaoLiberada, estado);
    }
}

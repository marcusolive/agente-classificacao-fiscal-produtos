namespace Api.Agente.Domain;

public static class ConfiancaPorFaixa
{
    public static int? Calcular(FaixaAutoridade? faixa, bool empate, bool possuiTrecho)
    {
        if (empate || faixa is null || !possuiTrecho)
        {
            return null;
        }

        return faixa switch
        {
            FaixaAutoridade.LegislacaoVigente => 90,
            FaixaAutoridade.NotasRgi => 80,
            FaixaAutoridade.CasoCorporativoConfirmado => 65,
            FaixaAutoridade.TextoFornecedor => 40,
            _ => null
        };
    }

    public static bool ExigeRevisao(int? confianca) => confianca is null || confianca < 70;
}

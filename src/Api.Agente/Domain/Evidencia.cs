namespace Api.Agente.Domain;

public sealed class Evidencia
{
    public required string Fonte { get; init; }
    public FaixaAutoridade FaixaAutoridade { get; init; }
    public string? Trecho { get; init; }
    public string? Localizacao { get; init; }
    public DateOnly Vigencia { get; init; }
    public string? Ncm { get; init; }
    public IReadOnlyDictionary<string, string> Tributos { get; init; } = new Dictionary<string, string>();
}

public sealed class ValorTratamento
{
    public SituacaoTratamento Situacao { get; init; }
    public string? Valor { get; init; }
    public string? Motivo { get; init; }
}

public sealed class Achado
{
    public ClasseAchado Classe { get; init; }
    public SeveridadeAchado Severidade { get; init; }
    public required string Descricao { get; init; }
}

public sealed class AtributosProduto
{
    public string? Funcao { get; init; }
    public string? Material { get; init; }
    public string? Composicao { get; init; }
    public string? Uso { get; init; }
    public string? Apresentacao { get; init; }
    public string? TermosTecnicos { get; init; }
}

public sealed class VersaoBase
{
    public required string Base { get; init; }
    public required string Versao { get; init; }
}

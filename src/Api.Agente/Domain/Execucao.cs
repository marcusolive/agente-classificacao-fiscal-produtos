namespace Api.Agente.Domain;

public sealed class ItemAnalise
{
    public required string ItemId { get; init; }
    public required string TextoOriginal { get; init; }
    public AtributosProduto Atributos { get; init; } = new();
    public SituacaoItem Situacao { get; set; }
    public bool RevisaoHumana { get; set; }
    public bool ImpactoRelevante { get; set; }
    public int? Confianca { get; set; }
    public string? NcmSugerida { get; set; }
    public List<string> Alternativas { get; } = [];
    public string? Incorreto { get; set; }
    public Dictionary<string, ValorTratamento> Tratamento { get; } = [];
    public string? XmlItem { get; set; }
    public List<string> DadoAusente { get; } = [];
    public List<Evidencia> Evidencias { get; } = [];
    public List<Achado> Achados { get; } = [];
}

public sealed class Execucao
{
    public Guid ExecucaoId { get; init; } = Guid.NewGuid();
    public string ContratoVersao { get; init; } = "1.0.0";
    public required string Usuario { get; init; }
    public required string Empresa { get; init; }
    public required string Papel { get; init; }
    public required string Finalidade { get; init; }
    public required string Uf { get; init; }
    public required string RegimeTributario { get; init; }
    public required string TipoOperacao { get; init; }
    public DateOnly DataVigencia { get; init; }
    public IntencaoAnalise Intencao { get; init; }
    public string VersaoAgente { get; init; } = VersaoDoAgente.Atual;
    public EstadoNota EstadoNota { get; set; }
    public List<ItemAnalise> Itens { get; } = [];
    public List<string> ValidacoesNaoRealizadas { get; } = [];
    public List<VersaoBase> VersoesBases { get; } = [];
}

public static class VersaoDoAgente
{
    public const string Atual = "1.0.0";
}

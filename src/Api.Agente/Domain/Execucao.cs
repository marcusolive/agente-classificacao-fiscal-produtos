using System.ComponentModel;

namespace Api.Agente.Domain;

[Description("Item analisado dentro da execução.")]
public sealed class ItemAnalise
{
    [Description("Identificador do item nesta execução.")]
    public required string ItemId { get; init; }

    [Description("Texto original usado na classificação.")]
    public required string TextoOriginal { get; init; }

    [Description("Atributos extraídos do texto do item.")]
    public AtributosProduto Atributos { get; init; } = new();

    [Description("Contexto efetivo do item. O valor informado no item prevalece sobre o da execução.")]
    public ContextoEfetivo? Contexto { get; set; }

    [Description("sugerido, divergente, dadoAusente, semEvidencia ou falhaTecnica.")]
    public SituacaoItem Situacao { get; set; }

    [Description("Verdadeiro quando a sugestão precisa de revisão humana.")]
    public bool RevisaoHumana { get; set; }

    [Description("Reservado. Permanece falso enquanto o critério de impacto não estiver definido.")]
    public bool ImpactoRelevante { get; set; }

    [Description("Confiança de 0 a 100. Nula quando não há vencedor único.")]
    public int? Confianca { get; set; }

    [Description("NCM sustentada pelas evidências. Não é certificação.")]
    public string? NcmSugerida { get; set; }

    [Description("Outros códigos encontrados nas evidências, sem vencer a ordenação.")]
    public List<string> Alternativas { get; init; } = [];

    [Description("Campo declarado que a auditoria considerou divergente.")]
    public string? Incorreto { get; set; }

    [Description("Tratamento por tributo: ipi, icms, cest, pis, cofins e ibsCbs.")]
    public Dictionary<string, ValorTratamento> Tratamento { get; init; } = [];

    [Description("XML somente do item, quando a intenção pede esse trecho.")]
    public string? XmlItem { get; set; }

    [Description("Dados essenciais que faltaram neste item.")]
    public List<string> DadoAusente { get; init; } = [];

    [Description("Evidências citadas para este item.")]
    public List<Evidencia> Evidencias { get; init; } = [];

    [Description("Achados registrados para este item.")]
    public List<Achado> Achados { get; init; } = [];
}

[Description("Resultado da execução no contrato 1.0.0.")]
public sealed class Execucao
{
    [Description("Identificador da execução. Use-o para consultar o resultado.")]
    public Guid ExecucaoId { get; init; } = Guid.NewGuid();

    [Description("Versão deste contrato. O valor atual é 1.0.0.")]
    public string ContratoVersao { get; init; } = ContratoApi.Versao;

    [Description("Usuário que solicitou a análise.")]
    public required string Usuario { get; init; }

    [Description("Empresa da execução.")]
    public required string Empresa { get; init; }

    [Description("Papel do solicitante.")]
    public required string Papel { get; init; }

    [Description("Finalidade informada no pedido.")]
    public required string Finalidade { get; init; }

    [Description("UF do contexto.")]
    public required string Uf { get; init; }

    [Description("Regime tributário do contexto.")]
    public required string RegimeTributario { get; init; }

    [Description("Tipo de operação do contexto.")]
    public required string TipoOperacao { get; init; }

    [Description("Data de vigência usada na consulta.")]
    public DateOnly DataVigencia { get; init; }

    [Description("Intenção pedida: sugerir ou auditar.")]
    public IntencaoAnalise Intencao { get; init; }

    [Description("Versão do agente que produziu o resultado.")]
    public string VersaoAgente { get; init; } = VersaoDoAgente.Atual;

    [Description("bloqueada, pendenteDeDado, falhaTecnica ou sugestaoLiberada.")]
    public EstadoNota EstadoNota { get; set; }

    [Description("Resultado de cada item. Um item incompleto não impede os demais.")]
    public List<ItemAnalise> Itens { get; init; } = [];

    [Description("Checagens que esta versão ainda não executa.")]
    public List<string> ValidacoesNaoRealizadas { get; init; } = [];

    [Description("Versões das bases citadas nesta execução.")]
    public List<VersaoBase> VersoesBases { get; init; } = [];
}

public static class VersaoDoAgente
{
    public const string Atual = "1.0.0";
}

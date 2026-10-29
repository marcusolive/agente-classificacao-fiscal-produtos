using System.ComponentModel;

namespace Api.Agente.Domain;

[Description("Contexto usado na classificação do item. Valor informado no item prevalece sobre o da execução.")]
public sealed class ContextoEfetivo
{
    [Description("UF efetiva do item.")]
    public required string Uf { get; init; }

    [Description("Regime tributário efetivo do item.")]
    public required string RegimeTributario { get; init; }

    [Description("Origem informada no item, quando existir.")]
    public string? Origem { get; init; }

    [Description("Destino informado no item, quando existir.")]
    public string? Destino { get; init; }

    [Description("Tipo de operação efetivo do item.")]
    public required string TipoOperacao { get; init; }

    [Description("Vigência usada na consulta deste item.")]
    public DateOnly DataVigencia { get; init; }

    [Description("Unidade informada, quando o item já vem separado.")]
    public string? Unidade { get; init; }

    [Description("Quantidade informada, quando o item já vem separado.")]
    public string? Quantidade { get; init; }

    [Description("NCM declarada. Usada na auditoria.")]
    public string? NcmDeclarada { get; init; }

    [Description("CEST declarado. Usado na auditoria quando houver evidência desse tributo.")]
    public string? CestDeclarado { get; init; }

    [Description("CFOP informado no item.")]
    public string? Cfop { get; init; }

    [Description("CST informado no item.")]
    public string? Cst { get; init; }
}

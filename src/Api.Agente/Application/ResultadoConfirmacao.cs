using System.ComponentModel;
using Api.Agente.Domain;

namespace Api.Agente.Application;

[Description("Confirmação validada no contrato 1.1.0. O caso não foi gravado.")]
public sealed class ResultadoConfirmacao
{
    public const string AvisoGravacao = "gravação na base de conhecimento não realizada";

    [Description("Versão deste contrato de confirmação. O valor atual é 1.1.0.")]
    public string ContratoVersao { get; init; } = ContratoApi.VersaoConfirmacao;

    [Description("Execução confirmada.")]
    public Guid ExecucaoId { get; init; }

    [Description("Empresa da execução de origem. A confirmação não aceita outra empresa.")]
    public required string Empresa { get; init; }

    [Description("Papel informado por quem confirmou.")]
    public required string PapelAutoridade { get; init; }

    [Description("Falso nesta versão. O caso ainda não é precedente.")]
    public bool Gravado { get; init; }

    [Description("Checagens deixadas para a base de conhecimento.")]
    public List<string> ValidacoesNaoRealizadas { get; } = [AvisoGravacao];

    [Description("Itens aceitos ou corrigidos dentro das evidências.")]
    public List<ItemConfirmado> Itens { get; } = [];
}

[Description("Item aceito ou corrigido sem sair das evidências da execução.")]
public sealed class ItemConfirmado
{
    [Description("Identificador do item na execução.")]
    public required string ItemId { get; init; }

    [Description("aceitar ou corrigir.")]
    public DecisaoConfirmacao Decisao { get; init; }

    [Description("NCM confirmada. Não é certificação.")]
    public required string Ncm { get; init; }

    [Description("Tratamentos confirmados, apenas valores sustentados ou presentes nas evidências.")]
    public Dictionary<string, string> Tributos { get; init; } = [];
}

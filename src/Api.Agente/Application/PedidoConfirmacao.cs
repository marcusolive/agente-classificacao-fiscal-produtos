using System.ComponentModel;
using Api.Agente.Domain;

namespace Api.Agente.Application;

[Description("Confirmação dos itens de uma execução. Não grava caso corporativo.")]
public sealed class PedidoConfirmacao
{
    [Description("Papel de quem confirma. Obrigatório. Não há catálogo fixo de cargos.")]
    public string PapelAutoridade { get; set; } = "";

    [Description("Itens da execução que serão aceitos ou corrigidos.")]
    public List<ItemPedidoConfirmacao> Itens { get; set; } = [];
}

[Description("Decisão de um item da execução.")]
public sealed class ItemPedidoConfirmacao
{
    [Description("Identificador do item na execução.")]
    public string ItemId { get; set; } = "";

    [Description("aceitar usa a NCM sustentada. corrigir exige outro valor presente nas evidências.")]
    public DecisaoConfirmacao Decisao { get; set; }

    [Description("NCM da correção. Obrigatória apenas quando a decisão é corrigir.")]
    public string? Ncm { get; set; }

    [Description("Tratamentos da correção. Cada valor precisa existir nas evidências do item.")]
    public Dictionary<string, string>? Tributos { get; set; }
}

public sealed class ExecucaoNaoEncontradaException() : Exception("Execução não encontrada.");

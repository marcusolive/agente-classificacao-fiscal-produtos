using Api.Agente.Domain;

namespace Api.Agente.Application;

public static class ContextoDoItem
{
    public static ContextoEfetivo Resolver(PedidoAnalise pedido, ItemPedido item) => new()
    {
        Uf = Texto(item.Uf, pedido.Uf),
        RegimeTributario = Texto(item.RegimeTributario, pedido.RegimeTributario),
        Origem = Opcional(item.Origem),
        Destino = Opcional(item.Destino),
        TipoOperacao = Texto(item.TipoOperacao, pedido.TipoOperacao),
        DataVigencia = item.DataVigencia is { } data && data != default ? data : pedido.DataVigencia,
        Unidade = Opcional(item.Unidade),
        Quantidade = Opcional(item.Quantidade),
        NcmDeclarada = Opcional(item.NcmDeclarada),
        CestDeclarado = Opcional(item.CestDeclarado),
        Cfop = Opcional(item.Cfop),
        Cst = Opcional(item.Cst)
    };

    public static IReadOnlyDictionary<string, string>? TributosDeclarados(ItemPedido item)
    {
        var tributos = item.TributosDeclarados is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(item.TributosDeclarados, StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(item.CestDeclarado) && !tributos.ContainsKey("cest"))
        {
            tributos["cest"] = item.CestDeclarado.Trim();
        }

        return tributos.Count == 0 ? null : tributos;
    }

    private static string Texto(string? doItem, string daExecucao) =>
        string.IsNullOrWhiteSpace(doItem) ? daExecucao.Trim() : doItem.Trim();

    private static string? Opcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}

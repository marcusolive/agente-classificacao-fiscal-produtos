using Api.Agente.Domain;

namespace Api.Agente.Application;

public sealed class PedidoAnalise
{
    public string Usuario { get; set; } = "";
    public string Empresa { get; set; } = "";
    public string Papel { get; set; } = "";
    public string Finalidade { get; set; } = "";
    public string Uf { get; set; } = "";
    public string RegimeTributario { get; set; } = "";
    public string TipoOperacao { get; set; } = "";
    public DateOnly DataVigencia { get; set; }
    public IntencaoAnalise Intencao { get; set; }
    public EntradaPedido Entrada { get; set; } = new();
}

public sealed class EntradaPedido
{
    public TipoEntrada Tipo { get; set; }
    public string? Texto { get; set; }
    public ArquivoPedido? Arquivo { get; set; }
    public List<ItemPedido> Itens { get; set; } = [];
}

public sealed class ArquivoPedido
{
    public string? Nome { get; set; }
    public string? ConteudoBase64 { get; set; }
}

public sealed class ItemPedido
{
    public string? Descricao { get; set; }
    public string? NcmDeclarada { get; set; }
    public Dictionary<string, string>? TributosDeclarados { get; set; }
}

public sealed class EntradaRejeitadaException(string message) : Exception(message);

public sealed class ConsultaIndisponivelException(string message) : Exception(message);

public sealed class SaidaModeloInvalidaException() : Exception("A saída do modelo não permaneceu nas evidências.");

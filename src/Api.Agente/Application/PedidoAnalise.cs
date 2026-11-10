using System.ComponentModel;
using Api.Agente.Domain;

namespace Api.Agente.Application;

[Description("Pedido da análise. Todos os campos de contexto são obrigatórios.")]
public sealed class PedidoAnalise
{
    [Description("Identificador do usuário que solicita a análise.")]
    public string Usuario { get; set; } = "";

    [Description("Empresa à qual a execução pertence.")]
    public string Empresa { get; set; } = "";

    [Description("Papel do solicitante nesta execução.")]
    public string Papel { get; set; } = "";

    [Description("Finalidade do pedido, como sugerir ou auditar.")]
    public string Finalidade { get; set; } = "";

    [Description("UF do contexto da execução.")]
    public string Uf { get; set; } = "";

    [Description("Regime tributário da execução.")]
    public string RegimeTributario { get; set; } = "";

    [Description("Tipo de operação, como entrada ou saída.")]
    public string TipoOperacao { get; set; } = "";

    [Description("Data em que a classificação precisa valer.")]
    public DateOnly DataVigencia { get; set; }

    [Description("sugerirNcm, auditarDeclarado, sugerirTratamento ou sugerirXmlItem.")]
    public IntencaoAnalise Intencao { get; set; }

    [Description("Texto, arquivo ou lista de itens a analisar.")]
    public EntradaPedido Entrada { get; set; } = new();
}

[Description("Conteúdo enviado para a análise.")]
public sealed class EntradaPedido
{
    [Description("texto, xmlNfe, planilha, pdf, laudo ou fichaTecnica.")]
    public TipoEntrada Tipo { get; set; }

    [Description("Descrição do produto quando a entrada é texto e não há lista de itens.")]
    public string? Texto { get; set; }

    [Description("Arquivo em Base64. Exigido quando o tipo não é texto.")]
    public ArquivoPedido? Arquivo { get; set; }

    [Description("Itens já separados. Cada item precisa de descrição.")]
    public List<ItemPedido> Itens { get; set; } = [];
}

[Description("Arquivo enviado para leitura. O layout interno não faz parte deste contrato.")]
public sealed class ArquivoPedido
{
    [Description("Nome original do arquivo.")]
    public string? Nome { get; set; }

    [Description("Conteúdo do arquivo em Base64.")]
    public string? ConteudoBase64 { get; set; }
}

[Description("Item informado pelo solicitante.")]
public sealed class ItemPedido
{
    [Description("Descrição original do produto, com função, composição e apresentação quando conhecidas.")]
    public string? Descricao { get; set; }

    [Description("NCM já declarada. Usada somente na auditoria.")]
    public string? NcmDeclarada { get; set; }

    [Description("CEST já declarado. Usado somente na auditoria.")]
    public string? CestDeclarado { get; set; }

    [Description("CFOP informado no item.")]
    public string? Cfop { get; set; }

    [Description("CST informado no item.")]
    public string? Cst { get; set; }

    [Description("Unidade informada, quando o item já vem separado.")]
    public string? Unidade { get; set; }

    [Description("Quantidade informada, quando o item já vem separado.")]
    public string? Quantidade { get; set; }

    [Description("UF específica do item. Quando vazia, vale a UF da execução.")]
    public string? Uf { get; set; }

    [Description("Regime específico do item. Quando vazio, vale o regime da execução.")]
    public string? RegimeTributario { get; set; }

    [Description("Origem informada no item.")]
    public string? Origem { get; set; }

    [Description("Destino informado no item.")]
    public string? Destino { get; set; }

    [Description("Operação específica do item. Quando vazia, vale a operação da execução.")]
    public string? TipoOperacao { get; set; }

    [Description("Vigência específica do item. Quando vazia, vale a vigência da execução.")]
    public DateOnly? DataVigencia { get; set; }

    [Description("Tratamentos já declarados, por tributo. Usados somente na auditoria.")]
    public Dictionary<string, string>? TributosDeclarados { get; set; }
}

public sealed class EntradaRejeitadaException : Exception
{
    public EntradaRejeitadaException(string message, ClasseAchado? classe = null) : base(message)
    {
        Classe = classe;
    }

    public ClasseAchado? Classe { get; }
}

public sealed class ConsultaIndisponivelException(string message) : Exception(message);

public sealed class SaidaModeloInvalidaException() : Exception("A saída do modelo não permaneceu nas evidências.");

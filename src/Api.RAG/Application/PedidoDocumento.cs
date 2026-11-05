using System.ComponentModel;
using Api.RAG.Domain;

namespace Api.RAG.Application;

[Description("Pedido de ingestão de uma versão. O texto informado não altera regra do sistema.")]
public sealed class PedidoDocumento
{
    [Description("Origem do documento.")]
    public string? Origem { get; set; }

    [Description("Identificação do documento dentro da origem.")]
    public string? Identificacao { get; set; }

    [Description("Versão. Conteúdo diferente na mesma versão é recusado.")]
    public string? Versao { get; set; }

    [Description("Faixa de autoridade. Somente as quatro faixas já decididas.")]
    public FaixaAutoridade? Faixa { get; set; }

    [Description("Início da vigência.")]
    public DateOnly? VigenciaInicial { get; set; }

    [Description("Fim da vigência. Vazio deixa o intervalo aberto.")]
    public DateOnly? VigenciaFinal { get; set; }

    [Description("texto, pdf ou xmlNotas.")]
    public FormatoDocumento? Formato { get; set; }

    [Description("Texto puro, quando o formato é texto.")]
    public string? Conteudo { get; set; }

    [Description("PDF ou XML de notas, em Base64.")]
    public string? Arquivo { get; set; }

    [Description("NCM informada pela origem. A Api.RAG não extrai código do texto.")]
    public string? Ncm { get; set; }

    [Description("Tributos informados pela origem.")]
    public Dictionary<string, string>? Tributos { get; set; }

    [Description("Decisão do caso confirmado.")]
    public DecisaoCaso? Decisao { get; set; }

    [Description("Papel informado na confirmação do caso.")]
    public string? PapelAutoridade { get; set; }

    [Description("Data da confirmação do caso.")]
    public DateOnly? DataConfirmacao { get; set; }

    [Description("Empresa da execução de origem.")]
    public string? Empresa { get; set; }

    [Description("Execução de origem do caso.")]
    public Guid? ExecucaoOrigem { get; set; }
}

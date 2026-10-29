using System.ComponentModel;

namespace Api.RAG.Domain;

[Description("Evidência devolvida pela ferramenta. Não é tabela e não inclui caso corporativo.")]
public sealed class EvidenciaConsultada
{
    [Description("Fonte no formato origem/identificacao/versao.")]
    public required string Fonte { get; init; }

    [Description("Faixa de autoridade do documento.")]
    public FaixaAutoridade FaixaAutoridade { get; init; }

    [Description("Trecho citado.")]
    public required string Trecho { get; init; }

    [Description("Localização do trecho, quando existir.")]
    public string? Localizacao { get; init; }

    [Description("Data pedida na consulta.")]
    public DateOnly Vigencia { get; init; }

    [Description("NCM informada na ingestão, quando existir.")]
    public string? Ncm { get; init; }

    [Description("Tributos informados na ingestão, quando existirem.")]
    public Dictionary<string, string> Tributos { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public static class ContratoApi
{
    public const string VersaoDocumento = "1.0.0";
    public const string Documento = "v1";
    public const string Titulo = "Base de conhecimento da Api.RAG";

    public const string Descricao =
        "Documento v1 da Api.RAG. A administração REST grava e lê documentos versionados. A consulta da análise é a ferramenta MCP consultar_conhecimento, com texto e vigência, e não é uma rota REST. A ferramenta não devolve caso corporativo. Não atesta conformidade fiscal. Uma mudança incompatível será publicada como v2.";
}

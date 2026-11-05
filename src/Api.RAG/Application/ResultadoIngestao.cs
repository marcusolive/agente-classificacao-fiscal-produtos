using System.ComponentModel;
using Api.RAG.Domain;

namespace Api.RAG.Application;

[Description("Resultado da ingestão. Não declara a versão consultável enquanto a busca vetorial não existir.")]
public sealed class ResultadoIngestao
{
    [Description("Identificador da versão gravada.")]
    public Guid DocumentoId { get; init; }

    [Description("Fonte no formato origem/identificacao/versao.")]
    public required string Fonte { get; init; }

    [Description("gravado, ignorado ou falha técnica.")]
    public EstadoDocumento Estado { get; init; }

    [Description("Verdadeiro só quando a busca vetorial e os embeddings existirem.")]
    public bool Consultavel { get; init; }
}

[Description("Recusa da ingestão, antes de gravar uma versão nova.")]
public sealed class ErroResposta
{
    [Description("Motivo da recusa.")]
    public required string Erro { get; init; }
}

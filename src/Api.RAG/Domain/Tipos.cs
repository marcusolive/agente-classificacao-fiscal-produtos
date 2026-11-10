using System.ComponentModel;

namespace Api.RAG.Domain;

[Description("Autoridade informada na ingestão. A Api.RAG não cria outra faixa.")]
public enum FaixaAutoridade
{
    LegislacaoVigente,
    NotasRgi,
    CasoCorporativoConfirmado,
    TextoFornecedor
}

[Description("Formato aceito nesta specification. XML de notas não tem schema oficial.")]
public enum FormatoDocumento
{
    Texto,
    Pdf,
    XmlNotas
}

[Description("Estado da versão depois da ingestão.")]
public enum EstadoDocumento
{
    Gravado,
    Ignorado,
    Recusado,
    FalhaTecnica
}

[Description("Decisão já confirmada do caso. Sugestão não confirmada não entra.")]
public enum DecisaoCaso
{
    Aceitar,
    Corrigir
}

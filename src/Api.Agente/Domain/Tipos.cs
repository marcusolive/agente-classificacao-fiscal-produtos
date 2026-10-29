using System.ComponentModel;

namespace Api.Agente.Domain;

[Description("Autoridade da evidência. A legislação vigente prevalece sobre as demais.")]
public enum FaixaAutoridade
{
    LegislacaoVigente = 1,
    NotasRgi = 2,
    CasoCorporativoConfirmado = 3,
    TextoFornecedor = 4
}

[Description("Estado da nota: bloqueada, pendente de dado, falha técnica ou sugestão liberada.")]
public enum EstadoNota
{
    Bloqueada,
    PendenteDeDado,
    FalhaTecnica,
    SugestaoLiberada
}

[Description("Situação do item após a análise.")]
public enum SituacaoItem
{
    Sugerido,
    Divergente,
    DadoAusente,
    SemEvidencia,
    FalhaTecnica
}

[Description("Classe do achado registrado.")]
public enum ClasseAchado
{
    ErroEstrutural,
    InconsistenciaNegocio,
    IndicioIa,
    AusenciaEvidencias,
    FalhaTecnica
}

[Description("Gravidade do achado.")]
public enum SeveridadeAchado
{
    Erro,
    Aviso
}

[Description("O que a execução deve fazer com os itens.")]
public enum IntencaoAnalise
{
    SugerirNcm,
    AuditarDeclarado,
    SugerirTratamento,
    SugerirXmlItem
}

[Description("Forma da entrada aceita nesta versão.")]
public enum TipoEntrada
{
    Texto,
    XmlNfe,
    Planilha,
    Pdf,
    Laudo,
    FichaTecnica
}

[Description("Se o tratamento foi sustentado por evidência ou ficou limitado.")]
public enum SituacaoTratamento
{
    Sustentado,
    Limitacao
}

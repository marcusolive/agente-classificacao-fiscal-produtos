namespace Api.Agente.Domain;

public enum FaixaAutoridade
{
    LegislacaoVigente = 1,
    NotasRgi = 2,
    CasoCorporativoConfirmado = 3,
    TextoFornecedor = 4
}

public enum EstadoNota
{
    Bloqueada,
    PendenteDeDado,
    FalhaTecnica,
    SugestaoLiberada
}

public enum SituacaoItem
{
    Sugerido,
    Divergente,
    DadoAusente,
    SemEvidencia,
    FalhaTecnica
}

public enum ClasseAchado
{
    ErroEstrutural,
    InconsistenciaNegocio,
    IndicioIa,
    AusenciaEvidencias,
    FalhaTecnica
}

public enum SeveridadeAchado
{
    Erro,
    Aviso
}

public enum IntencaoAnalise
{
    SugerirNcm,
    AuditarDeclarado,
    SugerirTratamento,
    SugerirXmlItem
}

public enum TipoEntrada
{
    Texto,
    XmlNfe,
    Planilha,
    Pdf,
    Laudo,
    FichaTecnica
}

public enum SituacaoTratamento
{
    Sustentado,
    Limitacao
}

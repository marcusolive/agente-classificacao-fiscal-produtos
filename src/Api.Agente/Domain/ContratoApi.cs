namespace Api.Agente.Domain;

public static class ContratoApi
{
    public const string VersaoAnalise = "1.0.0";
    public const string VersaoConfirmacao = "1.1.0";
    public const string VersaoDocumento = "1.1.0";
    public const string Versao = VersaoAnalise;
    public const string Documento = "v1";
    public const string Titulo = "Agente de classificação fiscal de produtos";

    public const string Descricao =
        "Documento v1. A análise de itens permanece no contrato 1.0.0. A confirmação de caso é o contrato 1.1.0 e não grava precedente. Não atesta conformidade fiscal. Uma mudança incompatível será publicada como v2.";
}

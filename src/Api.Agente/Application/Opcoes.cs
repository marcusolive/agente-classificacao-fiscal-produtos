namespace Api.Agente.Application;

public sealed class OpcoesAnalise
{
    public long? TamanhoMaximoBytes { get; set; }
    public int TentativasSaidaInvalida { get; set; } = 2;
    public int TimeoutModeloSegundos { get; set; } = 60;
    public int TimeoutConhecimentoSegundos { get; set; } = 15;
}

public sealed class OpcoesOpenAi
{
    public string Model { get; set; } = "";
    public string ApiKey { get; set; } = "";
}

public sealed class OpcoesConhecimento
{
    public string McpUrl { get; set; } = "";
    public string Ferramenta { get; set; } = "consultar_conhecimento";
}

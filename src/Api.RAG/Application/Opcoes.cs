namespace Api.RAG.Application;

public sealed class OpcoesIngestao
{
    public string Chave { get; set; } = "";
    public long? TamanhoMaximoBytes { get; set; }
}

public sealed class OpcoesEmbedding
{
    public string Model { get; set; } = "";
    public int? Dimensao { get; set; }
    public int? TopK { get; set; }
    public double? Limiar { get; set; }
    public int? TamanhoChunk { get; set; }
    public int? Sobreposicao { get; set; }
    public int Tentativas { get; set; } = 2;
    public int TimeoutSegundos { get; set; } = 60;
}

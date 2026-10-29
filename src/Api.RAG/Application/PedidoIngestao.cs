using System.Security.Cryptography;
using System.Text;
using Api.RAG.Domain;

namespace Api.RAG.Application;

public sealed class PedidoIngestao
{
    public string? Origem { get; set; }
    public string? Identificacao { get; set; }
    public string? Versao { get; set; }
    public FaixaAutoridade? Faixa { get; set; }
    public DateOnly? VigenciaInicial { get; set; }
    public FormatoDocumento? Formato { get; set; }
    public byte[]? Conteudo { get; set; }
}

public sealed class RecusaIngestaoException(string message) : Exception(message);

public static class HashConteudo
{
    public static string Calcular(byte[] conteudo) =>
        Convert.ToHexString(SHA256.HashData(conteudo)).ToLowerInvariant();
}

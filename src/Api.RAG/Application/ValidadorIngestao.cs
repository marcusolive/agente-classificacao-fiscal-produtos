using Api.RAG.Domain;
using Microsoft.Extensions.Options;

namespace Api.RAG.Application;

public sealed class ValidadorIngestao(IOptions<OpcoesIngestao> opcoes)
{
    public string Validar(PedidoIngestao pedido, string? chaveInformada, Documento? existente)
    {
        var chave = opcoes.Value.Chave;
        if (string.IsNullOrWhiteSpace(chave) || !string.Equals(chave, chaveInformada, StringComparison.Ordinal))
        {
            throw new RecusaIngestaoException("A chave de ingestão não confere.");
        }

        Exigir(pedido.Origem, "origem");
        Exigir(pedido.Identificacao, "identificacao");
        Exigir(pedido.Versao, "versao");
        if (pedido.Faixa is null || !Enum.IsDefined(pedido.Faixa.Value))
        {
            throw new RecusaIngestaoException("faixa é obrigatória.");
        }

        if (pedido.VigenciaInicial is null)
        {
            throw new RecusaIngestaoException("vigenciaInicial é obrigatória.");
        }

        if (pedido.Formato is null || !Enum.IsDefined(pedido.Formato.Value))
        {
            throw new RecusaIngestaoException("Formato de documento não aceito.");
        }

        var conteudo = pedido.Conteudo ?? [];
        if (pedido.Formato is FormatoDocumento.Pdf or FormatoDocumento.XmlNotas)
        {
            ValidarArquivo(conteudo);
        }
        else if (conteudo.Length == 0)
        {
            throw new RecusaIngestaoException("conteudo é obrigatório.");
        }

        var hash = HashConteudo.Calcular(conteudo);
        if (existente is not null && !string.Equals(existente.Hash, hash, StringComparison.OrdinalIgnoreCase))
        {
            throw new RecusaIngestaoException("Conteúdo diferente na mesma versão exige uma versão nova.");
        }

        return hash;
    }

    private void ValidarArquivo(byte[] conteudo)
    {
        if (opcoes.Value.TamanhoMaximoBytes is null)
        {
            throw new RecusaIngestaoException("Limite de arquivo não configurado.");
        }

        if (conteudo.Length == 0)
        {
            throw new RecusaIngestaoException("Arquivo vazio ou ausente.");
        }

        if (conteudo.Length > opcoes.Value.TamanhoMaximoBytes)
        {
            throw new RecusaIngestaoException("Arquivo acima do limite de processamento.");
        }
    }

    private static void Exigir(string? valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new RecusaIngestaoException($"{campo} é obrigatório.");
        }
    }
}

using Api.RAG.Domain;
using Microsoft.Extensions.Options;

namespace Api.RAG.Application;

public sealed class Segmentador(IOptions<OpcoesEmbedding> opcoes)
{
    public List<Chunk> Segmentar(Guid documentoId, string texto, string hash)
    {
        var tamanho = opcoes.Value.TamanhoChunk;
        var partes = tamanho is null or <= 0
            ? [texto]
            : Dividir(texto, tamanho.Value, Math.Max(0, opcoes.Value.Sobreposicao ?? 0));

        return partes.Select((parte, indice) => new Chunk
        {
            DocumentoId = documentoId,
            Posicao = indice,
            Texto = parte,
            HashDocumento = hash
        }).ToList();
    }

    private static List<string> Dividir(string texto, int tamanho, int sobreposicao)
    {
        var passo = Math.Max(1, tamanho - Math.Min(sobreposicao, tamanho - 1));
        var partes = new List<string>();
        for (var inicio = 0; inicio < texto.Length; inicio += passo)
        {
            var fim = Math.Min(texto.Length, inicio + tamanho);
            partes.Add(texto[inicio..fim]);
            if (fim == texto.Length)
            {
                break;
            }
        }

        return partes;
    }
}

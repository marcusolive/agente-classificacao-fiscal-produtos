using Api.RAG.Domain;
using Microsoft.Extensions.Options;

namespace Api.RAG.Application;

public sealed class ConsultaConhecimento(IRepositorioDocumento repositorio, IOptions<OpcoesEmbedding> opcoes)
{
    public async Task<IReadOnlyList<EvidenciaConsultada>> ConsultarAsync(string? texto, DateOnly vigencia, CancellationToken ct)
    {
        if (!IngestaoDocumento.BuscaConfigurada(opcoes.Value))
        {
            throw new BuscaIndisponivelException("A busca vetorial não está configurada.");
        }

        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new BuscaIndisponivelException("texto é obrigatório.");
        }

        var documentos = await repositorio.ListarTodosAsync(ct);
        var evidencias = new List<EvidenciaConsultada>();
        foreach (var documento in documentos.Where(d => Serve(d, vigencia)))
        {
            evidencias.AddRange(documento.Chunks.Select(chunk => new EvidenciaConsultada
            {
                Fonte = documento.Fonte,
                FaixaAutoridade = documento.Faixa,
                Trecho = chunk.Texto,
                Localizacao = chunk.Localizacao,
                Vigencia = vigencia,
                Ncm = documento.Ncm,
                Tributos = documento.Tributos
            }));
        }

        return evidencias.Take(opcoes.Value.TopK!.Value).ToList();
    }

    private static bool Serve(Documento documento, DateOnly vigencia) =>
        documento.Consultavel
        && documento.Estado == EstadoDocumento.Gravado
        && documento.Faixa != FaixaAutoridade.CasoCorporativoConfirmado
        && documento.VigenciaInicial <= vigencia
        && (documento.VigenciaFinal is null || documento.VigenciaFinal >= vigencia);
}

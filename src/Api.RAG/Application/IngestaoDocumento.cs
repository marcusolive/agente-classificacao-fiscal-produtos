using System.Text;
using Api.RAG.Domain;
using Microsoft.Extensions.Options;

namespace Api.RAG.Application;

public sealed class IngestaoDocumento(
    IRepositorioDocumento repositorio,
    ValidadorIngestao validador,
    ExtratorTexto extrator,
    Segmentador segmentador,
    IProvedorEmbedding embedding,
    IOptions<OpcoesEmbedding> opcoes)
{
    public async Task<ResultadoIngestao> IngerirAsync(PedidoDocumento entrada, string? chave, CancellationToken ct)
    {
        var pedido = Normalizar(entrada);
        var existente = await repositorio.ObterPorIdentidadeAsync(
            pedido.Origem?.Trim() ?? "",
            pedido.Identificacao?.Trim() ?? "",
            pedido.Versao?.Trim() ?? "",
            ct);
        var hash = validador.Validar(pedido, chave, existente);
        if (existente is not null)
        {
            return new ResultadoIngestao
            {
                DocumentoId = existente.DocumentoId,
                Fonte = existente.Fonte,
                Estado = EstadoDocumento.Ignorado,
                Consultavel = existente.Consultavel
            };
        }

        var texto = extrator.Extrair(pedido.Formato!.Value, pedido.Conteudo!);
        var documento = Novo(pedido, hash, texto);
        if (BuscaConfigurada(opcoes.Value))
        {
            try
            {
                await embedding.IncorporarAsync(documento, ct);
                documento.Consultavel = documento.Chunks.Count > 0;
                documento.Estado = EstadoDocumento.Gravado;
            }
            catch (BuscaIndisponivelException)
            {
                documento.Consultavel = false;
                documento.Estado = EstadoDocumento.FalhaTecnica;
            }
        }

        await repositorio.SalvarAsync(documento, ct);
        return new ResultadoIngestao
        {
            DocumentoId = documento.DocumentoId,
            Fonte = documento.Fonte,
            Estado = documento.Estado,
            Consultavel = documento.Consultavel
        };
    }

    public Task<Documento?> ObterAsync(Guid documentoId, CancellationToken ct) =>
        repositorio.ObterAsync(documentoId, ct);

    public Task<IReadOnlyList<Documento>> ListarAsync(string origem, string identificacao, CancellationToken ct) =>
        repositorio.ListarAsync(origem, identificacao, ct);

    private Documento Novo(PedidoIngestao pedido, string hash, string texto)
    {
        var documento = new Documento
        {
            Origem = pedido.Origem!.Trim(),
            Identificacao = pedido.Identificacao!.Trim(),
            Versao = pedido.Versao!.Trim(),
            Faixa = pedido.Faixa!.Value,
            VigenciaInicial = pedido.VigenciaInicial!.Value,
            VigenciaFinal = pedido.VigenciaFinal,
            Formato = pedido.Formato!.Value,
            Hash = hash,
            Estado = EstadoDocumento.Gravado,
            Consultavel = false,
            Ncm = Texto(pedido.Ncm),
            Tributos = pedido.Tributos ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            Decisao = pedido.Decisao,
            PapelAutoridade = Texto(pedido.PapelAutoridade),
            DataConfirmacao = pedido.DataConfirmacao,
            Empresa = Texto(pedido.Empresa),
            ExecucaoOrigem = pedido.ExecucaoOrigem
        };
        documento.Chunks = segmentador.Segmentar(documento.DocumentoId, texto, hash);
        return documento;
    }

    private static PedidoIngestao Normalizar(PedidoDocumento entrada)
    {
        byte[]? conteudo = entrada.Formato == FormatoDocumento.Texto
            ? Encoding.UTF8.GetBytes(entrada.Conteudo ?? "")
            : Decodificar(entrada.Arquivo);
        return new PedidoIngestao
        {
            Origem = entrada.Origem,
            Identificacao = entrada.Identificacao,
            Versao = entrada.Versao,
            Faixa = entrada.Faixa,
            VigenciaInicial = entrada.VigenciaInicial,
            VigenciaFinal = entrada.VigenciaFinal,
            Formato = entrada.Formato,
            Conteudo = conteudo,
            Ncm = entrada.Ncm,
            Tributos = entrada.Tributos,
            Decisao = entrada.Decisao,
            PapelAutoridade = entrada.PapelAutoridade,
            DataConfirmacao = entrada.DataConfirmacao,
            Empresa = entrada.Empresa,
            ExecucaoOrigem = entrada.ExecucaoOrigem
        };
    }

    private static byte[] Decodificar(string? arquivo)
    {
        if (string.IsNullOrWhiteSpace(arquivo))
        {
            return [];
        }

        try
        {
            return Convert.FromBase64String(arquivo);
        }
        catch (FormatException)
        {
            throw new RecusaIngestaoException("Arquivo ilegível.");
        }
    }

    public static bool BuscaConfigurada(OpcoesEmbedding opcoes) =>
        !string.IsNullOrWhiteSpace(opcoes.Model)
        && opcoes.Dimensao is > 0
        && opcoes.TopK is > 0
        && opcoes.Limiar is not null;

    private static string? Texto(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}

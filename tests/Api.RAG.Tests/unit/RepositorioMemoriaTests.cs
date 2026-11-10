using Api.RAG.Domain;
using Api.RAG.Infrastructure;

namespace Api.RAG.Tests.unit;

public class RepositorioMemoriaTests
{
    [Fact]
    public async Task Documento_gravado_volta_pela_identidade_sem_vetor()
    {
        var repositorio = new RepositorioDocumentoMemoria();
        var documento = new Documento
        {
            Origem = "oficial",
            Identificacao = "ncm",
            Versao = "1",
            Faixa = FaixaAutoridade.NotasRgi,
            VigenciaInicial = new DateOnly(2026, 1, 1),
            Formato = FormatoDocumento.Texto,
            Hash = "abc",
            Estado = EstadoDocumento.Gravado,
            Consultavel = false,
            Chunks =
            [
                new Chunk { Posicao = 0, Texto = "trecho", HashDocumento = "abc" }
            ]
        };

        await repositorio.SalvarAsync(documento, CancellationToken.None);
        var lido = await repositorio.ObterPorIdentidadeAsync("oficial", "ncm", "1", CancellationToken.None);

        Assert.NotNull(lido);
        Assert.Equal("oficial/ncm/1", lido.Fonte);
        Assert.False(lido.Consultavel);
        Assert.Equal("trecho", lido.Chunks[0].Texto);
        Assert.Equal(FaixaAutoridade.NotasRgi, lido.Faixa);
    }
}

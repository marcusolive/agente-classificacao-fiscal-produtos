using System.Text;
using Api.RAG.Application;
using Api.RAG.Domain;
using Microsoft.Extensions.Options;

namespace Api.RAG.Tests.unit;

public class ValidadorIngestaoTests
{
    private readonly ValidadorIngestao _validador = new(Options.Create(new OpcoesIngestao
    {
        Chave = "segredo",
        TamanhoMaximoBytes = null
    }));

    [Fact]
    public void Chave_ausente_recusa_antes_de_gravar()
    {
        var erro = Assert.Throws<RecusaIngestaoException>(() => _validador.Validar(Pedido(), "", null));
        Assert.Equal("A chave de ingestão não confere.", erro.Message);
    }

    [Fact]
    public void Campo_obrigatorio_ausente_e_recusado()
    {
        var pedido = Pedido();
        pedido.Origem = " ";
        var erro = Assert.Throws<RecusaIngestaoException>(() => _validador.Validar(pedido, "segredo", null));
        Assert.Equal("origem é obrigatório.", erro.Message);
    }

    [Fact]
    public void Formato_fora_da_lista_e_recusado()
    {
        var pedido = Pedido();
        pedido.Formato = (FormatoDocumento)9;
        var erro = Assert.Throws<RecusaIngestaoException>(() => _validador.Validar(pedido, "segredo", null));
        Assert.Equal("Formato de documento não aceito.", erro.Message);
    }

    [Fact]
    public void Arquivo_sem_limite_configurado_e_recusado()
    {
        var pedido = Pedido();
        pedido.Formato = FormatoDocumento.Pdf;
        pedido.Conteudo = [1, 2, 3];
        var erro = Assert.Throws<RecusaIngestaoException>(() => _validador.Validar(pedido, "segredo", null));
        Assert.Equal("Limite de arquivo não configurado.", erro.Message);
    }

    [Fact]
    public void Conteudo_diferente_na_mesma_versao_e_recusado()
    {
        var pedido = Pedido();
        var existente = new Documento
        {
            Origem = "oficial",
            Identificacao = "ncm",
            Versao = "1",
            Hash = HashConteudo.Calcular("outro"u8.ToArray())
        };
        var erro = Assert.Throws<RecusaIngestaoException>(() => _validador.Validar(pedido, "segredo", existente));
        Assert.Equal("Conteúdo diferente na mesma versão exige uma versão nova.", erro.Message);
    }

    [Fact]
    public void Mesmo_hash_nao_recusa()
    {
        var pedido = Pedido();
        var hash = HashConteudo.Calcular(pedido.Conteudo!);
        var existente = new Documento
        {
            Origem = "oficial",
            Identificacao = "ncm",
            Versao = "1",
            Hash = hash
        };

        Assert.Equal(hash, _validador.Validar(pedido, "segredo", existente));
    }

    private static PedidoIngestao Pedido() => new()
    {
        Origem = "oficial",
        Identificacao = "ncm",
        Versao = "1",
        Faixa = FaixaAutoridade.LegislacaoVigente,
        VigenciaInicial = new DateOnly(2026, 1, 1),
        Formato = FormatoDocumento.Texto,
        Conteudo = Encoding.UTF8.GetBytes("texto da base")
    };
}

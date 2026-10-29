using Api.Agente.Domain;
using Microsoft.Extensions.Options;

namespace Api.Agente.Application;

public sealed class ValidadorEntrada(IOptions<OpcoesAnalise> opcoes)
{
    public void Validar(PedidoAnalise pedido)
    {
        Exigir(pedido.Usuario, "usuario");
        Exigir(pedido.Empresa, "empresa");
        Exigir(pedido.Papel, "papel");
        Exigir(pedido.Finalidade, "finalidade");
        Exigir(pedido.Uf, "uf");
        Exigir(pedido.RegimeTributario, "regimeTributario");
        Exigir(pedido.TipoOperacao, "tipoOperacao");
        if (pedido.DataVigencia == default)
        {
            throw new EntradaRejeitadaException("dataVigencia é obrigatória.");
        }

        if (!Enum.IsDefined(pedido.Entrada.Tipo))
        {
            throw new EntradaRejeitadaException("Tipo de entrada não aceito.");
        }

        if (pedido.Entrada.Tipo == TipoEntrada.Texto)
        {
            var temTexto = !string.IsNullOrWhiteSpace(pedido.Entrada.Texto);
            var temItem = pedido.Entrada.Itens.Any(i => !string.IsNullOrWhiteSpace(i.Descricao));
            if (!temTexto && !temItem)
            {
                throw new EntradaRejeitadaException("A descrição do produto é obrigatória.");
            }

            return;
        }

        var arquivo = pedido.Entrada.Arquivo;
        if (arquivo is null || string.IsNullOrWhiteSpace(arquivo.ConteudoBase64))
        {
            throw new EntradaRejeitadaException("Arquivo vazio ou ausente.");
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(arquivo.ConteudoBase64);
        }
        catch (FormatException)
        {
            throw new EntradaRejeitadaException("Arquivo ilegível.");
        }

        if (bytes.Length == 0)
        {
            throw new EntradaRejeitadaException("Arquivo vazio ou ausente.");
        }

        var limite = opcoes.Value.TamanhoMaximoBytes;
        if (limite is null)
        {
            throw new EntradaRejeitadaException("Limite de arquivo não configurado.");
        }

        if (bytes.Length > limite)
        {
            throw new EntradaRejeitadaException("Arquivo acima do limite de processamento.");
        }
    }

    private static void Exigir(string valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new EntradaRejeitadaException($"{campo} é obrigatório.");
        }
    }
}

using Api.Agente.Domain;

namespace Api.Agente.Application;

public sealed class ConfirmadorDeCaso(IRepositorioExecucao repositorio)
{
    public async Task<ResultadoConfirmacao> ConfirmarAsync(Guid execucaoId, PedidoConfirmacao pedido, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(pedido.PapelAutoridade))
        {
            throw new EntradaRejeitadaException("papelAutoridade é obrigatório.");
        }

        if (pedido.Itens.Count == 0)
        {
            throw new EntradaRejeitadaException("Ao menos um item deve ser confirmado.");
        }

        if (pedido.Itens.Select(i => i.ItemId).Distinct(StringComparer.Ordinal).Count() != pedido.Itens.Count)
        {
            throw new EntradaRejeitadaException("Cada item só pode ser confirmado uma vez.");
        }

        var execucao = await repositorio.ObterAsync(execucaoId, ct) ?? throw new ExecucaoNaoEncontradaException();
        var resultado = new ResultadoConfirmacao
        {
            ExecucaoId = execucao.ExecucaoId,
            Empresa = execucao.Empresa,
            PapelAutoridade = pedido.PapelAutoridade.Trim()
        };

        foreach (var pedidoItem in pedido.Itens)
        {
            var item = execucao.Itens.FirstOrDefault(i => i.ItemId == pedidoItem.ItemId);
            if (item is null)
            {
                throw new EntradaRejeitadaException($"Item {pedidoItem.ItemId} não pertence à execução.");
            }

            resultado.Itens.Add(ConfirmarItem(item, pedidoItem));
        }

        return resultado;
    }

    private static ItemConfirmado ConfirmarItem(ItemAnalise item, ItemPedidoConfirmacao pedido)
    {
        var ncm = pedido.Decisao == DecisaoConfirmacao.Corrigir
            ? NcmCorrigida(item, pedido.Ncm)
            : NcmAceita(item);
        var tributos = Tributos(item, pedido);
        return new ItemConfirmado
        {
            ItemId = item.ItemId,
            Decisao = pedido.Decisao,
            Ncm = ncm,
            Tributos = tributos
        };
    }

    private static string NcmAceita(ItemAnalise item)
    {
        if (string.IsNullOrWhiteSpace(item.NcmSugerida))
        {
            throw new EntradaRejeitadaException($"O item {item.ItemId} não tem NCM sustentada para aceitar.");
        }

        return item.NcmSugerida;
    }

    private static string NcmCorrigida(ItemAnalise item, string? ncm)
    {
        if (string.IsNullOrWhiteSpace(ncm) || !NcmNaEvidencia(item, ncm))
        {
            throw new EntradaRejeitadaException($"A NCM informada não está nas evidências do item {item.ItemId}.");
        }

        return item.Evidencias.First(e => !string.IsNullOrWhiteSpace(e.Ncm) && CodigoFiscal.MesmaNcm(e.Ncm, ncm)).Ncm!;
    }

    private static Dictionary<string, string> Tributos(ItemAnalise item, ItemPedidoConfirmacao pedido)
    {
        if (pedido.Tributos is null)
        {
            return item.Tratamento
                .Where(p => p.Value.Situacao == SituacaoTratamento.Sustentado && !string.IsNullOrWhiteSpace(p.Value.Valor))
                .ToDictionary(p => p.Key, p => p.Value.Valor!);
        }

        foreach (var (tributo, valor) in pedido.Tributos)
        {
            if (!ValorNaEvidencia(item, tributo, valor))
            {
                throw new EntradaRejeitadaException($"O tratamento de {tributo} não está nas evidências do item {item.ItemId}.");
            }
        }

        return pedido.Tributos.ToDictionary(p => p.Key, p => p.Value.Trim());
    }

    private static bool NcmNaEvidencia(ItemAnalise item, string ncm) =>
        item.Evidencias.Any(e => !string.IsNullOrWhiteSpace(e.Ncm) && CodigoFiscal.MesmaNcm(e.Ncm, ncm));

    private static bool ValorNaEvidencia(ItemAnalise item, string tributo, string valor) =>
        item.Evidencias.Any(e =>
            e.Tributos.TryGetValue(tributo, out var encontrado)
            && string.Equals(encontrado?.Trim(), valor.Trim(), StringComparison.OrdinalIgnoreCase));
}

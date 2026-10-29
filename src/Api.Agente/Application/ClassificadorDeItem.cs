using Api.Agente.Domain;
using Microsoft.Extensions.Options;

namespace Api.Agente.Application;

public sealed class ClassificadorDeItem(
    IConsultaConhecimento conhecimento,
    IAnalisadorModelo analisador,
    IOptions<OpcoesAnalise> opcoes)
{
    private static readonly string[] Tributos = ["ipi", "icms", "cest", "pis", "cofins", "ibsCbs"];

    public async Task<ItemAnalise> ClassificarAsync(string itemId, string texto, DateOnly vigencia, CancellationToken ct)
    {
        var item = new ItemAnalise
        {
            ItemId = itemId,
            TextoOriginal = texto,
            Atributos = NormalizadorProduto.Normalizar(texto)
        };

        var ausentes = NormalizadorProduto.DadosEssenciaisAusentes(item.Atributos);
        if (ausentes.Count > 0)
        {
            item.Situacao = SituacaoItem.DadoAusente;
            item.DadoAusente.AddRange(ausentes);
            return item;
        }

        IReadOnlyList<Evidencia> evidencias;
        try
        {
            using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
            limite.CancelAfter(TimeSpan.FromSeconds(opcoes.Value.TimeoutConhecimentoSegundos));
            evidencias = await conhecimento.ConsultarAsync(texto, vigencia, limite.Token);
        }
        catch (Exception ex) when (ex is ConsultaIndisponivelException or OperationCanceledException)
        {
            return Falha(item, "A base de conhecimento não respondeu.");
        }

        var vigentes = evidencias.Where(e => e.Vigencia == vigencia).ToList();
        item.Evidencias.AddRange(vigentes);
        var ordenacao = OrdenacaoDeEvidencias.Ordenar(vigentes);
        if (ordenacao.Vencedora is null)
        {
            item.Situacao = SituacaoItem.SemEvidencia;
            item.RevisaoHumana = true;
            item.Achados.Add(new Achado
            {
                Classe = ClasseAchado.AusenciaEvidencias,
                Severidade = SeveridadeAchado.Aviso,
                Descricao = ordenacao.Empate
                    ? "Empate na mesma faixa de autoridade."
                    : "Não há evidência suficiente para escolher uma NCM."
            });
            PreencherTratamento(item, null);
            return item;
        }

        var confianca = ConfiancaPorFaixa.Calcular(
            ordenacao.Vencedora.FaixaAutoridade,
            false,
            !string.IsNullOrWhiteSpace(ordenacao.Vencedora.Trecho));

        SaidaModelo saida;
        try
        {
            saida = await analisador.AnalisarAsync(new ContextoModelo
            {
                Vencedora = ordenacao.Vencedora,
                Evidencias = ordenacao.Ordenadas
            }, ct);
        }
        catch (SaidaModeloInvalidaException)
        {
            return Falha(item, "A saída do modelo foi recusada.");
        }

        if (!ValidadorSaidaModelo.NcmPermitida(saida.Ncm, ordenacao.Ordenadas))
        {
            return Falha(item, "A NCM devolvida não está nas evidências da vigência.");
        }

        item.Situacao = SituacaoItem.Sugerido;
        item.NcmSugerida = saida.Ncm;
        item.Confianca = confianca;
        item.RevisaoHumana = ConfiancaPorFaixa.ExigeRevisao(confianca);
        item.Alternativas.AddRange(ordenacao.Ordenadas
            .Select(e => e.Ncm)
            .Where(n => !string.IsNullOrWhiteSpace(n) && n != saida.Ncm)
            .Distinct()!);
        PreencherTratamento(item, ordenacao.Vencedora);
        return item;
    }

    private static ItemAnalise Falha(ItemAnalise item, string descricao)
    {
        item.Situacao = SituacaoItem.FalhaTecnica;
        item.Achados.Add(new Achado
        {
            Classe = ClasseAchado.FalhaTecnica,
            Severidade = SeveridadeAchado.Erro,
            Descricao = descricao
        });
        return item;
    }

    private static void PreencherTratamento(ItemAnalise item, Evidencia? vencedora)
    {
        foreach (var tributo in Tributos)
        {
            if (vencedora is not null && vencedora.Tributos.TryGetValue(tributo, out var valor) && !string.IsNullOrWhiteSpace(valor))
            {
                item.Tratamento[tributo] = new ValorTratamento { Situacao = SituacaoTratamento.Sustentado, Valor = valor };
            }
            else
            {
                item.Tratamento[tributo] = new ValorTratamento
                {
                    Situacao = SituacaoTratamento.Limitacao,
                    Motivo = "Sem evidência na vigência."
                };
            }
        }
    }
}

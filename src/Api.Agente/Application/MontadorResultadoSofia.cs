using Api.Agente.Domain;

namespace Api.Agente.Application;

public static class MontadorResultadoSofia
{
    public static ResultadoAnaliseSofia DaAnalise(
        string idExecucao,
        string idSolicitacao,
        AgenteSofia agente,
        IReadOnlyList<DocumentoAnalisadoSofia> documentos)
    {
        var itens = documentos.Where(d => d.Execucao is not null).SelectMany(d => d.Execucao!.Itens).ToList();
        var achados = Achados(documentos);
        var falha = documentos.FirstOrDefault(d => d.CodigoErro is not null)
            ?? (itens.Any(i => i.Situacao == SituacaoItem.FalhaTecnica)
                ? new DocumentoAnalisadoSofia("", null, "falha-tecnica", MensagemFalha(itens))
                : null);
        var status = falha is not null
            ? "erro"
            : itens.Any(i => i.Situacao == SituacaoItem.DadoAusente) ? "parcial" : "concluido";
        var erro = falha is null
            ? null
            : new ErroSofia { Codigo = falha.CodigoErro!, Mensagem = falha.MensagemErro ?? "Falha técnica." };

        return new ResultadoAnaliseSofia
        {
            IdExecucao = idExecucao,
            IdSolicitacao = idSolicitacao,
            Agente = agente,
            Status = status,
            Analise = Texto(status, erro, itens, achados.Count),
            Achados = achados,
            Observacoes = Observacoes(documentos),
            Erro = erro
        };
    }

    private static List<AchadoSofia> Achados(IReadOnlyList<DocumentoAnalisadoSofia> documentos)
    {
        var lista = new List<AchadoSofia>();
        foreach (var documento in documentos)
        {
            if (documento.Execucao is null)
            {
                continue;
            }

            foreach (var item in documento.Execucao.Itens.Where(i => i.Situacao == SituacaoItem.Divergente))
            {
                var trechos = item.Evidencias
                    .Select(e => e.Trecho)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t!)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                if (trechos.Count == 0)
                {
                    continue;
                }

                // A mensagem deixa claro o que foi declarado e o que se sustenta na evidência.
                var baseDescricao = !string.IsNullOrWhiteSpace(item.Incorreto)
                    ? $"{item.Incorreto} não se sustenta na evidência."
                    : item.Achados.FirstOrDefault(a => a.Classe == ClasseAchado.InconsistenciaNegocio)?.Descricao
                        ?? $"Item {item.ItemId} divergente.";
                var descricao = string.IsNullOrWhiteSpace(item.NcmSugerida)
                    ? baseDescricao
                    : $"{baseDescricao} NCM sustentada: {item.NcmSugerida}.";

                lista.Add(new AchadoSofia
                {
                    IdAchado = $"{documento.IdDocumento}-{item.ItemId}",
                    Descricao = descricao,
                    IdDocumentos = [documento.IdDocumento],
                    Evidencias = trechos
                });
            }
        }

        return lista;
    }

    private static string Texto(string status, ErroSofia? erro, List<ItemAnalise> itens, int divergencias)
    {
        var partes = new List<string>();
        if (erro is not null)
        {
            partes.Add(erro.Mensagem);
        }

        if (status == "parcial")
        {
            var campos = itens.SelectMany(i => i.DadoAusente).Distinct(StringComparer.Ordinal).ToArray();
            partes.Add(campos.Length == 0
                ? "Faltam dados essenciais em ao menos um item."
                : "Faltam dados essenciais: " + string.Join(", ", campos) + ".");
        }

        var ncms = itens.Select(i => i.NcmSugerida).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.Ordinal).ToArray();
        if (ncms.Length > 0)
        {
            partes.Add("NCM sustentada: " + string.Join(", ", ncms) + ".");
        }

        if (divergencias > 0)
        {
            partes.Add($"Divergências: {divergencias}.");
        }

        return partes.Count == 0
            ? AtendenteSofia.AnaliseSemAchado
            : string.Join(" ", partes);
    }

    private static List<string> Observacoes(IReadOnlyList<DocumentoAnalisadoSofia> documentos)
    {
        var lista = documentos.Select(d => d.IdDocumento).ToList();
        lista.AddRange(documentos
            .Where(d => d.Execucao is not null)
            .Select(d => "execucao:" + d.Execucao!.ExecucaoId));
        return lista;
    }

    private static string MensagemFalha(List<ItemAnalise> itens) =>
        itens.SelectMany(i => i.Achados).Select(a => a.Descricao).FirstOrDefault(d => !string.IsNullOrWhiteSpace(d))
        ?? "Falha técnica.";
}

public sealed record DocumentoAnalisadoSofia(
    string IdDocumento,
    Execucao? Execucao,
    string? CodigoErro,
    string? MensagemErro);

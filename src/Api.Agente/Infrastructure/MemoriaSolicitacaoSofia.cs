using System.Collections.Concurrent;
using Api.Agente.Domain;

namespace Api.Agente.Infrastructure;

public sealed class MemoriaSolicitacaoSofia
{
    private readonly ConcurrentDictionary<Guid, Item> _itens = new();

    public (string TarefaId, ResultadoAnaliseSofia Resultado) ObterOuCriar(
        Guid idSolicitacao,
        Func<ResultadoAnaliseSofia> criar)
    {
        var item = _itens.GetOrAdd(idSolicitacao, _ => new Item(Guid.NewGuid().ToString("D"), criar()));
        return (item.TarefaId, item.Resultado);
    }

    private sealed record Item(string TarefaId, ResultadoAnaliseSofia Resultado);
}

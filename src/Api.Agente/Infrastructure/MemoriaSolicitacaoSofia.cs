using System.Collections.Concurrent;
using Api.Agente.Domain;

namespace Api.Agente.Infrastructure;

public sealed class MemoriaSolicitacaoSofia
{
    private readonly ConcurrentDictionary<Guid, Item> _itens = new();
    private readonly ConcurrentDictionary<string, Item> _porTarefa = new(StringComparer.Ordinal);

    public bool Tentar(Guid idSolicitacao, out (string TarefaId, ResultadoAnaliseSofia Resultado) resultado)
    {
        if (_itens.TryGetValue(idSolicitacao, out var item))
        {
            resultado = (item.TarefaId, item.Resultado);
            return true;
        }

        resultado = default;
        return false;
    }

    public bool TentarPorTarefa(string tarefaId, out ResultadoAnaliseSofia resultado)
    {
        if (_porTarefa.TryGetValue(tarefaId, out var item))
        {
            resultado = item.Resultado;
            return true;
        }

        resultado = default!;
        return false;
    }

    public (string TarefaId, ResultadoAnaliseSofia Resultado) Guardar(Guid idSolicitacao, ResultadoAnaliseSofia resultado)
    {
        var item = _itens.GetOrAdd(idSolicitacao, _ => new Item(Guid.NewGuid().ToString("D"), resultado));
        _porTarefa.TryAdd(item.TarefaId, item);
        return (item.TarefaId, item.Resultado);
    }

    private sealed record Item(string TarefaId, ResultadoAnaliseSofia Resultado);
}

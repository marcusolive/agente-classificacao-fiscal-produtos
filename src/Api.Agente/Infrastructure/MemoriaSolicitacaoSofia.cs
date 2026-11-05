using System.Collections.Concurrent;
using Api.Agente.Domain;

namespace Api.Agente.Infrastructure;

public sealed class MemoriaSolicitacaoSofia
{
    private readonly ConcurrentDictionary<Guid, Item> _itens = new();

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

    public (string TarefaId, ResultadoAnaliseSofia Resultado) Guardar(Guid idSolicitacao, ResultadoAnaliseSofia resultado)
    {
        var tarefa = Guid.NewGuid().ToString("D");
        var item = _itens.GetOrAdd(idSolicitacao, _ => new Item(tarefa, resultado));
        return (item.TarefaId, item.Resultado);
    }

    private sealed record Item(string TarefaId, ResultadoAnaliseSofia Resultado);
}

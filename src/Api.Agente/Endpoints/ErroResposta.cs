using System.ComponentModel;
using Api.Agente.Domain;

namespace Api.Agente.Endpoints;

public sealed class ErroResposta
{
    [Description("Motivo objetivo da recusa, antes de qualquer classificação.")]
    public required string Erro { get; init; }

    [Description("erroEstrutural quando o documento não pode ser lido. Ausente quando a recusa é só condição de processamento.")]
    public ClasseAchado? Classe { get; init; }
}

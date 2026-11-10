using System.Text;
using Api.Agente.Domain;

namespace Api.Agente.Application;

public sealed class ValidadorSolicitacaoSofia
{
    public string? Validar(SolicitacaoAnaliseSofia? pedido, OpcoesSofia opcoes)
    {
        if (pedido is null)
        {
            return "Solicitação ausente.";
        }

        if (pedido.VersaoContrato != "1.0")
        {
            return "versaoContrato deve ser 1.0.";
        }

        if (!Guid.TryParse(pedido.IdExecucao, out _))
        {
            return "idExecucao deve ser UUID.";
        }

        if (!Guid.TryParse(pedido.IdSolicitacao, out _))
        {
            return "idSolicitacao deve ser UUID.";
        }

        if (string.IsNullOrWhiteSpace(pedido.Usuario?.Nome))
        {
            return "usuario.nome é obrigatório.";
        }

        if (string.IsNullOrWhiteSpace(pedido.Pergunta))
        {
            return "pergunta é obrigatória.";
        }

        if (pedido.Documentos is not { Count: > 0 })
        {
            return "documentos é obrigatório.";
        }

        foreach (var documento in pedido.Documentos)
        {
            if (string.IsNullOrWhiteSpace(documento.IdDocumento))
            {
                return "idDocumento é obrigatório.";
            }

            if (string.IsNullOrWhiteSpace(documento.Nome))
            {
                return "nome do documento é obrigatório.";
            }

            if (string.IsNullOrWhiteSpace(documento.Texto))
            {
                return "texto do documento é obrigatório.";
            }

            if (opcoes.TamanhoMaximoBytes is long limite
                && Encoding.UTF8.GetByteCount(documento.Texto) > limite)
            {
                return "O texto do documento excede o limite configurado.";
            }
        }

        return null;
    }
}

public sealed class SolicitacaoInvalidaException(string message) : Exception(message);

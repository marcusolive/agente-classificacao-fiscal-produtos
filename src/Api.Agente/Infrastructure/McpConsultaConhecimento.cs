using System.Text.Json;
using Api.Agente.Application;
using Api.Agente.Domain;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Api.Agente.Infrastructure;

public sealed class McpConsultaConhecimento(IOptions<OpcoesConhecimento> opcoes, IOptions<OpcoesAnalise> analise) : IConsultaConhecimento
{
    public async Task<IReadOnlyList<Evidencia>> ConsultarAsync(string texto, DateOnly vigencia, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(opcoes.Value.McpUrl))
        {
            throw new ConsultaIndisponivelException("A consulta de conhecimento não está configurada.");
        }

        try
        {
            using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
            limite.CancelAfter(TimeSpan.FromSeconds(analise.Value.TimeoutConhecimentoSegundos));
            await using var cliente = await McpClient.CreateAsync(
                new HttpClientTransport(new HttpClientTransportOptions
                {
                    Endpoint = new Uri(opcoes.Value.McpUrl),
                    Name = "Api.RAG"
                }),
                cancellationToken: limite.Token);

            var resultado = await cliente.CallToolAsync(
                opcoes.Value.Ferramenta,
                new Dictionary<string, object?>
                {
                    ["texto"] = texto,
                    ["vigencia"] = vigencia.ToString("yyyy-MM-dd")
                },
                cancellationToken: limite.Token);

            if (resultado.IsError == true)
            {
                throw new ConsultaIndisponivelException("A ferramenta de conhecimento retornou erro.");
            }

            var conteudo = string.Concat(resultado.Content.OfType<TextContentBlock>().Select(b => b.Text));
            return JsonSerializer.Deserialize<List<Evidencia>>(conteudo, JsonDaExecucao.Opcoes) ?? [];
        }
        catch (ConsultaIndisponivelException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ConsultaIndisponivelException(ex.Message);
        }
    }
}

using System.Net.Http.Json;
using System.Text.Json;

namespace Api.Agente.Tests.integration;

internal static class ClienteDeAnalise
{
    public static async Task<JsonElement> PostarAsync(HttpClient cliente, string intencao, object entrada)
    {
        var resposta = await cliente.PostAsJsonAsync("/v1/analises", new
        {
            usuario = "ana",
            empresa = "empresa-1",
            papel = "analista",
            finalidade = "analise",
            uf = "SP",
            regimeTributario = "lucro_real",
            tipoOperacao = "saida",
            dataVigencia = "2026-01-01",
            intencao,
            entrada
        });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<JsonElement>())!;
    }

    public static string Completo(string descricao) =>
        $"""
        Função: unir peças
        Composição: aço
        Apresentação: unidade
        {descricao}
        """;

    public static string SemComposicao(string descricao) =>
        $"""
        Função: unir peças
        Apresentação: unidade
        {descricao}
        """;
}

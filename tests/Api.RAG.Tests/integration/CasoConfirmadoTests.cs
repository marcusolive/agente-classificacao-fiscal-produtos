using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.RAG.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Api.RAG.Tests.integration;

public class CasoConfirmadoTests : IDisposable
{
    private readonly FabricaApi _fabrica = new() { BuscaConfigurada = true };

    [Fact]
    public async Task Caso_completo_fica_na_administracao_e_fora_da_consulta()
    {
        var cliente = ClienteDocumento.Criar(_fabrica);
        var execucao = Guid.NewGuid();
        var criado = await ClienteDocumento.PostarAsync(cliente, new
        {
            origem = "empresa-a",
            identificacao = "caso-1",
            versao = "1",
            faixa = "casoCorporativoConfirmado",
            vigenciaInicial = "2026-01-01",
            formato = "texto",
            conteudo = "caso confirmado da empresa",
            decisao = "aceitar",
            papelAutoridade = "revisor",
            dataConfirmacao = "2026-02-01",
            empresa = "empresa-a",
            execucaoOrigem = execucao
        });
        var id = criado.GetProperty("documentoId").GetGuid();
        var lido = await cliente.GetFromJsonAsync<JsonElement>($"/v1/documentos/{id}");

        Assert.Equal("empresa-a", lido.GetProperty("empresa").GetString());
        Assert.Equal("revisor", lido.GetProperty("papelAutoridade").GetString());
        Assert.Equal(execucao, lido.GetProperty("execucaoOrigem").GetGuid());

        var consulta = _fabrica.Services.GetRequiredService<ConsultaConhecimento>();
        var evidencias = await consulta.ConsultarAsync("caso", new DateOnly(2026, 1, 15), CancellationToken.None);
        Assert.DoesNotContain(evidencias, e => e.Fonte == "empresa-a/caso-1/1");
    }

    [Fact]
    public async Task Caso_sem_papel_e_recusado()
    {
        var cliente = ClienteDocumento.Criar(_fabrica);
        var resposta = await cliente.PostAsJsonAsync("/v1/documentos", new
        {
            origem = "empresa-a",
            identificacao = "caso-2",
            versao = "1",
            faixa = "casoCorporativoConfirmado",
            vigenciaInicial = "2026-01-01",
            formato = "texto",
            conteudo = "caso incompleto",
            decisao = "aceitar",
            dataConfirmacao = "2026-02-01",
            empresa = "empresa-a",
            execucaoOrigem = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Lista_separa_as_versoes()
    {
        var cliente = ClienteDocumento.Criar(_fabrica);
        await ClienteDocumento.PostarAsync(cliente, ClienteDocumento.Texto("primeira", "10", "2026-01-01", "2026-01-31"));
        await ClienteDocumento.PostarAsync(cliente, ClienteDocumento.Texto("segunda", "11", "2026-02-01", "2026-02-28"));
        var lista = await cliente.GetFromJsonAsync<JsonElement>("/v1/documentos?origem=oficial&identificacao=ncm");

        Assert.Equal(2, lista.GetArrayLength());
        Assert.NotEqual(
            lista[0].GetProperty("chunks")[0].GetProperty("texto").GetString(),
            lista[1].GetProperty("chunks")[0].GetProperty("texto").GetString());
    }

    public void Dispose() => _fabrica.Dispose();
}

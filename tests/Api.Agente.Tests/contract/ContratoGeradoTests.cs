using System.Text.Json;
using Api.Agente.Tests.integration;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace Api.Agente.Tests.contract;

public class ContratoGeradoTests : IClassFixture<FabricaApi>
{
    private readonly FabricaApi _fabrica;

    public ContratoGeradoTests(FabricaApi fabrica) => _fabrica = fabrica;

    [Fact]
    public async Task Yaml_versionado_sai_do_documento_publicado()
    {
        var bytes = await _fabrica.CreateClient().GetByteArrayAsync("/openapi/v1.json");
        using var stream = new MemoryStream(bytes);
        var lido = await OpenApiDocument.LoadAsync(stream, "json");
        Assert.NotNull(lido.Document);

        using var json = JsonDocument.Parse(bytes);
        var versaoDocumento = json.RootElement.GetProperty("openapi").GetString() ?? "";
        var spec = versaoDocumento.StartsWith("3.1", StringComparison.Ordinal)
            ? OpenApiSpecVersion.OpenApi3_1
            : OpenApiSpecVersion.OpenApi3_0;
        var yaml = (await lido.Document.SerializeAsYamlAsync(spec)).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (!yaml.EndsWith('\n'))
        {
            yaml += "\n";
        }

        var caminho = CaminhoDoContrato();
        var gravado = (await File.ReadAllTextAsync(caminho)).Replace("\r\n", "\n", StringComparison.Ordinal);
        Assert.Equal(yaml, gravado);
        Assert.Contains("1.0.0", yaml, StringComparison.Ordinal);
        Assert.Contains("/v1/analises", yaml, StringComparison.Ordinal);
    }

    private static string CaminhoDoContrato()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "specs")))
        {
            dir = dir.Parent;
        }

        return Path.Combine(dir!.FullName, "specs", "001-execucao-analise-fiscal", "contracts", "analise-v1.openapi.yaml");
    }
}

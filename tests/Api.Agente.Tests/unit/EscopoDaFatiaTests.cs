namespace Api.Agente.Tests.unit;

public class EscopoDaFatiaTests
{
    [Fact]
    public void Nao_grava_caso_nem_acessa_vetor_memoria_consumo_ou_a2a()
    {
        var raiz = DiretorioDoCodigo();
        var fontes = Directory.GetFiles(raiz, "*.cs", SearchOption.AllDirectories)
            .Select(File.ReadAllText)
            .ToList();
        var texto = string.Join('\n', fontes);

        Assert.DoesNotContain("Pgvector", texto, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("NpgsqlVector", texto, StringComparison.Ordinal);
        Assert.DoesNotContain("A2A", texto, StringComparison.Ordinal);
        Assert.DoesNotContain("memoria conversacional", texto, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("consumo de token", texto, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("INSERT INTO caso", texto, StringComparison.OrdinalIgnoreCase);
    }

    private static string DiretorioDoCodigo()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "Api.Agente")))
        {
            dir = dir.Parent;
        }

        return Path.Combine(dir!.FullName, "src", "Api.Agente");
    }
}

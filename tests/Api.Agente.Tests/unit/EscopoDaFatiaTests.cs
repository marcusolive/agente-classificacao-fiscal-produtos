namespace Api.Agente.Tests.unit;

public class EscopoDaFatiaTests
{
    [Fact]
    public void Nao_grava_caso_nem_acessa_vetor_memoria_ou_consumo()
    {
        var raiz = DiretorioDoCodigo();
        var arquivos = Directory.GetFiles(raiz, "*.cs", SearchOption.AllDirectories);
        var texto = string.Join('\n', arquivos.Select(File.ReadAllText));
        var analise = string.Join('\n', arquivos
            .Where(caminho => !EhSuperficieSofia(caminho))
            .Select(File.ReadAllText));

        Assert.DoesNotContain("Pgvector", texto, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("NpgsqlVector", texto, StringComparison.Ordinal);
        Assert.DoesNotContain("A2A", analise, StringComparison.Ordinal);
        Assert.DoesNotContain("memoria conversacional", texto, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("consumo de token", texto, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("INSERT INTO caso", texto, StringComparison.OrdinalIgnoreCase);
    }

    private static bool EhSuperficieSofia(string caminho) =>
        caminho.Contains($"{Path.DirectorySeparatorChar}SofiaEndpoints.cs", StringComparison.Ordinal)
        || caminho.Contains($"{Path.DirectorySeparatorChar}ContratoSofia.cs", StringComparison.Ordinal)
        || caminho.Contains($"{Path.DirectorySeparatorChar}AtendenteSofia.cs", StringComparison.Ordinal)
        || caminho.Contains($"{Path.DirectorySeparatorChar}ValidadorSolicitacaoSofia.cs", StringComparison.Ordinal)
        || caminho.Contains($"{Path.DirectorySeparatorChar}MemoriaSolicitacaoSofia.cs", StringComparison.Ordinal)
        || caminho.EndsWith($"{Path.DirectorySeparatorChar}Program.cs", StringComparison.Ordinal)
        || caminho.EndsWith($"{Path.DirectorySeparatorChar}ContratoApi.cs", StringComparison.Ordinal);

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

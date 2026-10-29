using System.Collections.Concurrent;
using System.Text.Json;
using Api.Agente.Application;
using Api.Agente.Domain;
using Npgsql;
using NpgsqlTypes;

namespace Api.Agente.Infrastructure;

public sealed class RepositorioExecucaoMemoria : IRepositorioExecucao
{
    private readonly ConcurrentDictionary<Guid, string> _dados = new();

    public Task SalvarAsync(Execucao execucao, CancellationToken ct)
    {
        _dados[execucao.ExecucaoId] = JsonSerializer.Serialize(execucao, JsonDaExecucao.Opcoes);
        return Task.CompletedTask;
    }

    public Task<Execucao?> ObterAsync(Guid execucaoId, CancellationToken ct)
    {
        if (!_dados.TryGetValue(execucaoId, out var json))
        {
            return Task.FromResult<Execucao?>(null);
        }

        return Task.FromResult(JsonSerializer.Deserialize<Execucao>(json, JsonDaExecucao.Opcoes));
    }
}

public static class JsonDaExecucao
{
    public static readonly JsonSerializerOptions Opcoes = Criar();

    private static JsonSerializerOptions Criar()
    {
        var opcoes = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        opcoes.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return opcoes;
    }
}

public sealed class PostgresRepositorioExecucao(string conexao) : IRepositorioExecucao
{
    public async Task GarantirEsquemaAsync(CancellationToken ct)
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Persistence", "001_execucao.sql");
        var sql = await File.ReadAllTextAsync(caminho, ct);
        await using var conn = new NpgsqlConnection(conexao);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task SalvarAsync(Execucao execucao, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(conexao);
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);
        const string insert = """
            INSERT INTO execucao (id, resultado)
            VALUES (@id, @resultado::jsonb)
            ON CONFLICT (id) DO UPDATE SET resultado = EXCLUDED.resultado;
            """;
        await using var cmd = new NpgsqlCommand(insert, conn, tx);
        cmd.Parameters.AddWithValue("id", execucao.ExecucaoId);
        cmd.Parameters.Add(new NpgsqlParameter("resultado", NpgsqlDbType.Jsonb)
        {
            Value = JsonSerializer.Serialize(execucao, JsonDaExecucao.Opcoes)
        });
        await cmd.ExecuteNonQueryAsync(ct);
        foreach (var item in execucao.Itens)
        {
            await using var itemCmd = new NpgsqlCommand(
                "INSERT INTO item_analise (execucao_id, item_id, situacao, ncm_sugerida) VALUES (@execucao, @item, @situacao, @ncm)",
                conn, tx);
            itemCmd.Parameters.AddWithValue("execucao", execucao.ExecucaoId);
            itemCmd.Parameters.AddWithValue("item", item.ItemId);
            itemCmd.Parameters.AddWithValue("situacao", item.Situacao.ToString());
            itemCmd.Parameters.AddWithValue("ncm", (object?)item.NcmSugerida ?? DBNull.Value);
            await itemCmd.ExecuteNonQueryAsync(ct);

            foreach (var evidencia in item.Evidencias)
            {
                await using var evCmd = new NpgsqlCommand(
                    "INSERT INTO evidencia_citada (execucao_id, item_id, fonte, faixa, trecho, vigencia) VALUES (@execucao, @item, @fonte, @faixa, @trecho, @vigencia)",
                    conn, tx);
                evCmd.Parameters.AddWithValue("execucao", execucao.ExecucaoId);
                evCmd.Parameters.AddWithValue("item", item.ItemId);
                evCmd.Parameters.AddWithValue("fonte", evidencia.Fonte);
                evCmd.Parameters.AddWithValue("faixa", evidencia.FaixaAutoridade.ToString());
                evCmd.Parameters.AddWithValue("trecho", (object?)evidencia.Trecho ?? DBNull.Value);
                evCmd.Parameters.AddWithValue("vigencia", evidencia.Vigencia);
                await evCmd.ExecuteNonQueryAsync(ct);
            }

            foreach (var achado in item.Achados)
            {
                await using var achadoCmd = new NpgsqlCommand(
                    "INSERT INTO achado (execucao_id, item_id, classe, severidade, descricao) VALUES (@execucao, @item, @classe, @severidade, @descricao)",
                    conn, tx);
                achadoCmd.Parameters.AddWithValue("execucao", execucao.ExecucaoId);
                achadoCmd.Parameters.AddWithValue("item", item.ItemId);
                achadoCmd.Parameters.AddWithValue("classe", achado.Classe.ToString());
                achadoCmd.Parameters.AddWithValue("severidade", achado.Severidade.ToString());
                achadoCmd.Parameters.AddWithValue("descricao", achado.Descricao);
                await achadoCmd.ExecuteNonQueryAsync(ct);
            }
        }

        await tx.CommitAsync(ct);
    }

    public async Task<Execucao?> ObterAsync(Guid execucaoId, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(conexao);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT resultado::text FROM execucao WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", execucaoId);
        var valor = await cmd.ExecuteScalarAsync(ct);
        if (valor is not string json)
        {
            return null;
        }

        return JsonSerializer.Deserialize<Execucao>(json, JsonDaExecucao.Opcoes);
    }
}

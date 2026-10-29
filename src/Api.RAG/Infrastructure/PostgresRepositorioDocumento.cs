using System.Text.Json;
using Api.RAG.Application;
using Api.RAG.Domain;
using Npgsql;
using NpgsqlTypes;

namespace Api.RAG.Infrastructure;

public sealed class PostgresRepositorioDocumento(string conexao) : IRepositorioDocumento
{
    public async Task GarantirEsquemaAsync(CancellationToken ct)
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Persistence", "001_documento.sql");
        var sql = await File.ReadAllTextAsync(caminho, ct);
        await using var conn = new NpgsqlConnection(conexao);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public Task<Documento?> ObterAsync(Guid documentoId, CancellationToken ct) =>
        UmAsync("SELECT conteudo::text FROM documento WHERE documento_id = @id", "id", documentoId, ct);

    public async Task<Documento?> ObterPorIdentidadeAsync(string origem, string identificacao, string versao, CancellationToken ct)
    {
        await using var conn = await AbrirAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT conteudo::text FROM documento WHERE origem = @origem AND identificacao = @identificacao AND versao = @versao",
            conn);
        cmd.Parameters.AddWithValue("origem", origem);
        cmd.Parameters.AddWithValue("identificacao", identificacao);
        cmd.Parameters.AddWithValue("versao", versao);
        var valor = await cmd.ExecuteScalarAsync(ct);
        return valor is string json ? JsonSerializer.Deserialize<Documento>(json, JsonDaBase.Opcoes) : null;
    }

    public async Task<IReadOnlyList<Documento>> ListarAsync(string origem, string identificacao, CancellationToken ct)
    {
        await using var conn = await AbrirAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT conteudo::text FROM documento WHERE origem = @origem AND identificacao = @identificacao ORDER BY versao",
            conn);
        cmd.Parameters.AddWithValue("origem", origem);
        cmd.Parameters.AddWithValue("identificacao", identificacao);
        var lista = new List<Documento>();
        await using var leitor = await cmd.ExecuteReaderAsync(ct);
        while (await leitor.ReadAsync(ct))
        {
            var documento = JsonSerializer.Deserialize<Documento>(leitor.GetString(0), JsonDaBase.Opcoes);
            if (documento is not null)
            {
                lista.Add(documento);
            }
        }

        return lista;
    }

    public async Task SalvarAsync(Documento documento, CancellationToken ct)
    {
        await using var conn = await AbrirAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);
        await using (var buscar = new NpgsqlCommand(
            "SELECT documento_id FROM documento WHERE origem = @origem AND identificacao = @identificacao AND versao = @versao",
            conn, tx))
        {
            buscar.Parameters.AddWithValue("origem", documento.Origem);
            buscar.Parameters.AddWithValue("identificacao", documento.Identificacao);
            buscar.Parameters.AddWithValue("versao", documento.Versao);
            if (await buscar.ExecuteScalarAsync(ct) is Guid id)
            {
                documento.DocumentoId = id;
                foreach (var chunk in documento.Chunks)
                {
                    chunk.DocumentoId = id;
                }
            }
        }

        const string upsert = """
            INSERT INTO documento (
                documento_id, origem, identificacao, versao, faixa, vigencia_inicial, vigencia_final,
                formato, hash, estado, fonte, consultavel, conteudo)
            VALUES (
                @id, @origem, @identificacao, @versao, @faixa, @inicio, @fim,
                @formato, @hash, @estado, @fonte, @consultavel, @conteudo::jsonb)
            ON CONFLICT (origem, identificacao, versao) DO UPDATE SET
                hash = EXCLUDED.hash,
                estado = EXCLUDED.estado,
                fonte = EXCLUDED.fonte,
                consultavel = EXCLUDED.consultavel,
                conteudo = EXCLUDED.conteudo;
            """;
        await using (var cmd = new NpgsqlCommand(upsert, conn, tx))
        {
            cmd.Parameters.AddWithValue("id", documento.DocumentoId);
            cmd.Parameters.AddWithValue("origem", documento.Origem);
            cmd.Parameters.AddWithValue("identificacao", documento.Identificacao);
            cmd.Parameters.AddWithValue("versao", documento.Versao);
            cmd.Parameters.AddWithValue("faixa", documento.Faixa.ToString());
            cmd.Parameters.AddWithValue("inicio", documento.VigenciaInicial);
            cmd.Parameters.AddWithValue("fim", (object?)documento.VigenciaFinal ?? DBNull.Value);
            cmd.Parameters.AddWithValue("formato", documento.Formato.ToString());
            cmd.Parameters.AddWithValue("hash", documento.Hash);
            cmd.Parameters.AddWithValue("estado", documento.Estado.ToString());
            cmd.Parameters.AddWithValue("fonte", documento.Fonte);
            cmd.Parameters.AddWithValue("consultavel", documento.Consultavel);
            cmd.Parameters.Add(new NpgsqlParameter("conteudo", NpgsqlDbType.Jsonb)
            {
                Value = JsonSerializer.Serialize(documento, JsonDaBase.Opcoes)
            });
            await cmd.ExecuteNonQueryAsync(ct);
        }

        await using (var apagar = new NpgsqlCommand("DELETE FROM chunk WHERE documento_id = @id", conn, tx))
        {
            apagar.Parameters.AddWithValue("id", documento.DocumentoId);
            await apagar.ExecuteNonQueryAsync(ct);
        }

        foreach (var chunk in documento.Chunks)
        {
            await using var inserir = new NpgsqlCommand(
                """
                INSERT INTO chunk (chunk_id, documento_id, posicao, texto, localizacao, hash_documento)
                VALUES (@id, @documento, @posicao, @texto, @localizacao, @hash)
                """,
                conn, tx);
            inserir.Parameters.AddWithValue("id", chunk.ChunkId);
            inserir.Parameters.AddWithValue("documento", documento.DocumentoId);
            inserir.Parameters.AddWithValue("posicao", chunk.Posicao);
            inserir.Parameters.AddWithValue("texto", chunk.Texto);
            inserir.Parameters.AddWithValue("localizacao", (object?)chunk.Localizacao ?? DBNull.Value);
            inserir.Parameters.AddWithValue("hash", chunk.HashDocumento);
            await inserir.ExecuteNonQueryAsync(ct);
        }

        await tx.CommitAsync(ct);
    }

    private async Task<Documento?> UmAsync(string sql, string nome, Guid id, CancellationToken ct)
    {
        await using var conn = await AbrirAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue(nome, id);
        var valor = await cmd.ExecuteScalarAsync(ct);
        return valor is string json ? JsonSerializer.Deserialize<Documento>(json, JsonDaBase.Opcoes) : null;
    }

    private async Task<NpgsqlConnection> AbrirAsync(CancellationToken ct)
    {
        var conn = new NpgsqlConnection(conexao);
        await conn.OpenAsync(ct);
        return conn;
    }
}

using System.Text.Json;
using Api.RAG.Application;
using Api.RAG.Domain;
using Npgsql;
using NpgsqlTypes;
using Pgvector;
using Pgvector.Npgsql;

namespace Api.RAG.Infrastructure;

public sealed class PostgresRepositorioDocumento : IRepositorioDocumento, IDisposable
{
    private readonly NpgsqlDataSource _fonte;
    private readonly int? _dimensao;

    public PostgresRepositorioDocumento(string conexao, int? dimensao)
    {
        var construtor = new NpgsqlDataSourceBuilder(conexao);
        construtor.UseVector();
        _fonte = construtor.Build();
        _dimensao = dimensao is > 0 ? dimensao : null;
    }

    public void Dispose() => _fonte.Dispose();

    public async Task GarantirEsquemaAsync(CancellationToken ct)
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Persistence", "001_documento.sql");
        var sql = await File.ReadAllTextAsync(caminho, ct);
        await using var conn = await AbrirAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(ct);
        if (_dimensao is int dimensao)
        {
            await GarantirColunaAsync(conn, dimensao, ct);
        }
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

    public async Task<int> RemoverPorOrigemAsync(string origem, CancellationToken ct)
    {
        await using var conn = await AbrirAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);
        await using (var chunks = new NpgsqlCommand(
            "DELETE FROM chunk WHERE documento_id IN (SELECT documento_id FROM documento WHERE origem = @origem)",
            conn, tx))
        {
            chunks.Parameters.AddWithValue("origem", origem);
            await chunks.ExecuteNonQueryAsync(ct);
        }

        int removidos;
        await using (var docs = new NpgsqlCommand("DELETE FROM documento WHERE origem = @origem", conn, tx))
        {
            docs.Parameters.AddWithValue("origem", origem);
            removidos = await docs.ExecuteNonQueryAsync(ct);
        }

        await tx.CommitAsync(ct);
        return removidos;
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

    public async Task<IReadOnlyList<Documento>> ListarTodosAsync(CancellationToken ct)
    {
        await using var conn = await AbrirAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT conteudo::text FROM documento", conn);
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

    public async Task<IReadOnlyList<EvidenciaConsultada>> BuscarAsync(
        float[] consulta,
        DateOnly vigencia,
        int topK,
        double limiar,
        CancellationToken ct)
    {
        if (_dimensao is null)
        {
            throw new BuscaIndisponivelException("A busca vetorial não está configurada.");
        }

        const string sql = """
            SELECT d.fonte, d.faixa, d.conteudo::text, c.texto, c.localizacao
            FROM chunk c
            JOIN documento d ON d.documento_id = c.documento_id
            WHERE d.consultavel
              AND d.estado = 'Gravado'
              AND d.faixa <> 'CasoCorporativoConfirmado'
              AND d.vigencia_inicial <= @data
              AND (d.vigencia_final IS NULL OR d.vigencia_final >= @data)
              AND c.embedding IS NOT NULL
              AND 1 - (c.embedding <=> @consulta) >= @limiar
            ORDER BY c.embedding <=> @consulta, d.fonte, c.posicao
            LIMIT @topk
            """;
        await using var conn = await AbrirAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("data", vigencia);
        cmd.Parameters.AddWithValue("consulta", new Vector(consulta));
        cmd.Parameters.AddWithValue("limiar", limiar);
        cmd.Parameters.AddWithValue("topk", topK);
        var lista = new List<EvidenciaConsultada>();
        await using var leitor = await cmd.ExecuteReaderAsync(ct);
        while (await leitor.ReadAsync(ct))
        {
            var documento = JsonSerializer.Deserialize<Documento>(leitor.GetString(2), JsonDaBase.Opcoes);
            if (!Enum.TryParse<FaixaAutoridade>(leitor.GetString(1), out var faixa))
            {
                continue;
            }

            lista.Add(new EvidenciaConsultada
            {
                Fonte = leitor.GetString(0),
                FaixaAutoridade = faixa,
                Trecho = leitor.GetString(3),
                Localizacao = leitor.IsDBNull(4) ? null : leitor.GetString(4),
                Vigencia = vigencia,
                Ncm = documento?.Ncm,
                Tributos = documento?.Tributos ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            });
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
            var sqlChunk = _dimensao is null
                ? """
                INSERT INTO chunk (chunk_id, documento_id, posicao, texto, localizacao, hash_documento)
                VALUES (@id, @documento, @posicao, @texto, @localizacao, @hash)
                """
                : """
                INSERT INTO chunk (chunk_id, documento_id, posicao, texto, localizacao, hash_documento, embedding)
                VALUES (@id, @documento, @posicao, @texto, @localizacao, @hash, @embedding)
                """;
            await using var inserir = new NpgsqlCommand(sqlChunk, conn, tx);
            inserir.Parameters.AddWithValue("id", chunk.ChunkId);
            inserir.Parameters.AddWithValue("documento", documento.DocumentoId);
            inserir.Parameters.AddWithValue("posicao", chunk.Posicao);
            inserir.Parameters.AddWithValue("texto", chunk.Texto);
            inserir.Parameters.AddWithValue("localizacao", (object?)chunk.Localizacao ?? DBNull.Value);
            inserir.Parameters.AddWithValue("hash", chunk.HashDocumento);
            if (_dimensao is not null)
            {
                inserir.Parameters.AddWithValue(
                    "embedding",
                    chunk.Embedding is null ? DBNull.Value : new Vector(chunk.Embedding));
            }

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

    private async Task GarantirColunaAsync(NpgsqlConnection conn, int dimensao, CancellationToken ct)
    {
        const string ler = """
            SELECT a.atttypmod
            FROM pg_attribute a
            JOIN pg_class c ON c.oid = a.attrelid
            WHERE c.relname = 'chunk' AND a.attname = 'embedding' AND NOT a.attisdropped
            """;
        await using var leitura = new NpgsqlCommand(ler, conn);
        var atual = await leitura.ExecuteScalarAsync(ct);
        if (atual is int typmod)
        {
            if (typmod != dimensao)
            {
                throw new BuscaIndisponivelException("A dimensão configurada não confere com a coluna de embedding.");
            }

            return;
        }

        await using var criar = new NpgsqlCommand($"ALTER TABLE chunk ADD COLUMN IF NOT EXISTS embedding vector({dimensao})", conn);
        await criar.ExecuteNonQueryAsync(ct);
    }

    private async Task<NpgsqlConnection> AbrirAsync(CancellationToken ct) =>
        await _fonte.OpenConnectionAsync(ct);
}

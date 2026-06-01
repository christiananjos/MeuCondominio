using Dapper;
using MeuCondominio.Domain.Entities;
using MeuCondominio.Domain.Interfaces.Repositories;
using Npgsql;

namespace MeuCondominio.Infrastructure.Persistence.Repositories;

public sealed class MoradorRepository : IMoradorRepository
{
    private readonly string _connectionString;

    public MoradorRepository(string connectionString)
        => _connectionString = connectionString;

    public async Task<Morador?> ObterPorIdAsync(Guid id, Guid condominioId, CancellationToken ct = default)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        const string sql = """
            SELECT id, condominio_id, bloco, apartamento, nome_responsavel, whatsapp, created_at
            FROM   moradores
            WHERE  id = @Id AND condominio_id = @CondominioId
            LIMIT  1
            """;

        var row = await conn.QueryFirstOrDefaultAsync<MoradorRow>(sql, new { Id = id, CondominioId = condominioId });
        return row is null ? null : MapearMorador(row);
    }

    public async Task<Morador?> ObterPorUnidadeAsync(Guid condominioId, string bloco, string apartamento, CancellationToken ct = default)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        const string sql = """
            SELECT id, condominio_id, bloco, apartamento, nome_responsavel, whatsapp, created_at
            FROM   moradores
            WHERE  condominio_id = @CondominioId
              AND  bloco         = @Bloco
              AND  apartamento   = @Apartamento
            LIMIT  1
            """;

        var row = await conn.QueryFirstOrDefaultAsync<MoradorRow>(
            sql, new { CondominioId = condominioId, Bloco = bloco.ToUpper(), Apartamento = apartamento.ToUpper() });
        return row is null ? null : MapearMorador(row);
    }

    public async Task<IEnumerable<Morador>> ListarAsync(Guid condominioId, CancellationToken ct = default)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        const string sql = """
            SELECT id, condominio_id, bloco, apartamento, nome_responsavel, whatsapp, created_at
            FROM   moradores
            WHERE  condominio_id = @CondominioId
            ORDER  BY bloco, apartamento
            """;

        var rows = await conn.QueryAsync<MoradorRow>(sql, new { CondominioId = condominioId });
        return rows.Select(MapearMorador);
    }

    public async Task AdicionarAsync(Morador morador, CancellationToken ct = default)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        const string sql = """
            INSERT INTO moradores (id, condominio_id, bloco, apartamento, nome_responsavel, whatsapp, created_at)
            VALUES (@Id, @CondominioId, @Bloco, @Apartamento, @NomeResponsavel, @WhatsApp, @CreatedAt)
            """;

        await conn.ExecuteAsync(sql, new
        {
            morador.Id,
            morador.CondominioId,
            morador.Bloco,
            morador.Apartamento,
            morador.NomeResponsavel,
            morador.WhatsApp,
            morador.CreatedAt
        });
    }

    public async Task AtualizarAsync(Morador morador, CancellationToken ct = default)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        const string sql = """
            UPDATE moradores
            SET nome_responsavel = @NomeResponsavel,
                whatsapp         = @WhatsApp
            WHERE id            = @Id
              AND condominio_id  = @CondominioId
            """;

        await conn.ExecuteAsync(sql, new
        {
            morador.NomeResponsavel,
            morador.WhatsApp,
            morador.Id,
            morador.CondominioId
        });
    }

    private static Morador MapearMorador(MoradorRow row)
        => Morador.Criar(row.CondominiId, row.Bloco, row.Apartamento, row.NomeResponsavel, row.WhatsApp);

    private sealed record MoradorRow(
        Guid     Id,
        Guid     CondominiId,
        string   Bloco,
        string   Apartamento,
        string   NomeResponsavel,
        string   WhatsApp,
        DateTime CreatedAt);
}

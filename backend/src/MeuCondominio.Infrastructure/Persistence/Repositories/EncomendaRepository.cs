using Dapper;
using MeuCondominio.Domain.Entities;
using MeuCondominio.Domain.Interfaces.Repositories;
using Npgsql;

namespace MeuCondominio.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de encomendas usando Dapper para queries otimizadas.
/// O condominio_id é sempre incluído como filtro (multi-tenant safety net).
/// </summary>
public sealed class EncomendaRepository : IEncomendaRepository
{
    private readonly string _connectionString;

    public EncomendaRepository(string connectionString)
        => _connectionString = connectionString;

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<Encomenda?> ObterPorIdAsync(Guid id, Guid condominioId, CancellationToken ct = default)
    {
        using var conn = CreateConnection();
        const string sql = """
            SELECT e.*, m.*
            FROM encomendas e
            INNER JOIN moradores m ON m.id = e.morador_id
            WHERE e.id = @Id AND e.condominio_id = @CondominioId
            LIMIT 1
            """;

        // Multi-map Dapper para hidratar o objeto Morador na encomenda
        var result = await conn.QueryAsync<dynamic>(sql, new { Id = id, CondominioId = condominioId });
        // Mapeamento manual — em produção considere um ORM completo ou AutoMapper
        return result.FirstOrDefault() is not null ? MapearEncomenda(result.First()) : null;
    }

    public async Task<IEnumerable<Encomenda>> ListarPendentesAsync(Guid condominioId, CancellationToken ct = default)
    {
        using var conn = CreateConnection();
        const string sql = """
            SELECT e.*, m.*
            FROM encomendas e
            INNER JOIN moradores m ON m.id = e.morador_id
            WHERE e.condominio_id = @CondominioId
              AND e.status IN ('Pendente', 'Notificado')
            ORDER BY e.data_recebimento DESC
            """;

        var rows = await conn.QueryAsync<dynamic>(sql, new { CondominioId = condominioId });
        return rows.Select(r => (Encomenda)MapearEncomenda(r));
    }

    public async Task AdicionarAsync(Encomenda encomenda, CancellationToken ct = default)
    {
        using var conn = CreateConnection();
        const string sql = """
            INSERT INTO encomendas
                (id, condominio_id, morador_id, tipo_encomenda, status, codigo_retirada, data_recebimento)
            VALUES
                (@Id, @CondominioId, @MoradorId, @TipoEncomenda, @Status, @CodigoRetirada, @DataRecebimento)
            """;

        await conn.ExecuteAsync(sql, new
        {
            encomenda.Id,
            encomenda.CondominioId,
            encomenda.MoradorId,
            encomenda.TipoEncomenda,
            encomenda.Status,
            encomenda.CodigoRetirada,
            encomenda.DataRecebimento
        });
    }

    public async Task AtualizarAsync(Encomenda encomenda, CancellationToken ct = default)
    {
        using var conn = CreateConnection();
        const string sql = """
            UPDATE encomendas
            SET status        = @Status,
                data_retirada = @DataRetirada
            WHERE id            = @Id
              AND condominio_id  = @CondominioId
            """;

        await conn.ExecuteAsync(sql, new
        {
            encomenda.Status,
            encomenda.DataRetirada,
            encomenda.Id,
            encomenda.CondominioId
        });
    }

    // Mapeamento manual simples — substitua por ORM em produção
    private static Encomenda MapearEncomenda(dynamic row)
    {
        // Em produção, use reflection-safe mapping ou um ORM
        throw new NotImplementedException(
            "Implemente o mapeamento de colunas para a entidade Encomenda conforme seu schema.");
    }
}

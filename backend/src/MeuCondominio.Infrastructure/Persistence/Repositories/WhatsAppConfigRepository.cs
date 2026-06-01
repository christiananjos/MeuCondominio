using Dapper;
using MeuCondominio.Domain.Entities;
using MeuCondominio.Domain.Interfaces.Repositories;
using Npgsql;

namespace MeuCondominio.Infrastructure.Persistence.Repositories;

public sealed class WhatsAppConfigRepository : IWhatsAppConfigRepository
{
    private readonly string _connectionString;

    public WhatsAppConfigRepository(string connectionString)
        => _connectionString = connectionString;

    public async Task<WhatsAppConfig?> ObterPorCondominioAsync(Guid condominioId, CancellationToken ct = default)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        const string sql = """
            SELECT id, condominio_id, meta_token, phone_number_id, template_name, updated_at
            FROM   whatsapp_config
            WHERE  condominio_id = @CondominioId
            LIMIT  1
            """;

        var row = await conn.QueryFirstOrDefaultAsync<WhatsAppConfigRow>(sql, new { CondominioId = condominioId });
        if (row is null) return null;

        // Reconstitui a entidade de domínio
        return WhatsAppConfig.Criar(row.CondominiId, row.MetaToken, row.PhoneNumberId, row.TemplateName);
    }

    public async Task AdicionarOuAtualizarAsync(WhatsAppConfig config, CancellationToken ct = default)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        const string sql = """
            INSERT INTO whatsapp_config (id, condominio_id, meta_token, phone_number_id, template_name, updated_at)
            VALUES (@Id, @CondominioId, @MetaToken, @PhoneNumberId, @TemplateName, NOW())
            ON CONFLICT (condominio_id) DO UPDATE
            SET meta_token      = EXCLUDED.meta_token,
                phone_number_id = EXCLUDED.phone_number_id,
                template_name   = EXCLUDED.template_name,
                updated_at      = NOW()
            """;

        await conn.ExecuteAsync(sql, new
        {
            config.Id,
            config.CondominioId,
            config.MetaToken,
            config.PhoneNumberId,
            config.TemplateName
        });
    }

    private sealed record WhatsAppConfigRow(
        Guid   Id,
        Guid   CondominiId,
        string MetaToken,
        string PhoneNumberId,
        string TemplateName,
        DateTime UpdatedAt);
}

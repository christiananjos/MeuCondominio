using MeuCondominio.Domain.Entities;

namespace MeuCondominio.Domain.Interfaces.Repositories;

public interface IWhatsAppConfigRepository
{
    Task<WhatsAppConfig?> ObterPorCondominioAsync(Guid condominioId, CancellationToken ct = default);
    Task AdicionarOuAtualizarAsync(WhatsAppConfig config, CancellationToken ct = default);
}

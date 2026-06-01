using MeuCondominio.Domain.Entities;

namespace MeuCondominio.Domain.Interfaces.Repositories;

public interface IMoradorRepository
{
    Task<Morador?> ObterPorIdAsync(Guid id, Guid condominioId, CancellationToken ct = default);
    Task<Morador?> ObterPorUnidadeAsync(Guid condominioId, string bloco, string apartamento, CancellationToken ct = default);
    Task<IEnumerable<Morador>> ListarAsync(Guid condominioId, CancellationToken ct = default);
    Task AdicionarAsync(Morador morador, CancellationToken ct = default);
    Task AtualizarAsync(Morador morador, CancellationToken ct = default);
}

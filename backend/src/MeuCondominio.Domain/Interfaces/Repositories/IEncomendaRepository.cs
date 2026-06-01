using MeuCondominio.Domain.Entities;

namespace MeuCondominio.Domain.Interfaces.Repositories;

public interface IEncomendaRepository
{
    Task<Encomenda?> ObterPorIdAsync(Guid id, Guid condominioId, CancellationToken ct = default);
    Task<IEnumerable<Encomenda>> ListarPendentesAsync(Guid condominioId, CancellationToken ct = default);
    Task AdicionarAsync(Encomenda encomenda, CancellationToken ct = default);
    Task AtualizarAsync(Encomenda encomenda, CancellationToken ct = default);
}

using MediatR;
using MeuCondominio.Application.DTOs;
using MeuCondominio.Domain.Interfaces.Repositories;

namespace MeuCondominio.Application.UseCases.Encomendas.ListarPendentes;

public sealed class ListarPendentesHandler
    : IRequestHandler<ListarPendentesQuery, IEnumerable<EncomendaDto>>
{
    private readonly IEncomendaRepository _encomendaRepo;

    public ListarPendentesHandler(IEncomendaRepository encomendaRepo)
        => _encomendaRepo = encomendaRepo;

    public async Task<IEnumerable<EncomendaDto>> Handle(
        ListarPendentesQuery query,
        CancellationToken    cancellationToken)
    {
        var encomendas = await _encomendaRepo.ListarPendentesAsync(query.CondominioId, cancellationToken);

        return encomendas.Select(e => new EncomendaDto(
            e.Id,
            e.MoradorId,
            e.Morador?.NomeResponsavel ?? string.Empty,
            e.Morador?.Bloco           ?? string.Empty,
            e.Morador?.Apartamento     ?? string.Empty,
            e.TipoEncomenda,
            e.Status,
            e.DataRecebimento,
            e.DataRetirada));
    }
}

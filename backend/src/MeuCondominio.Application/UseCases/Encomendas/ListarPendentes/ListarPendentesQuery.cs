using MediatR;
using MeuCondominio.Application.DTOs;

namespace MeuCondominio.Application.UseCases.Encomendas.ListarPendentes;

public sealed record ListarPendentesQuery(Guid CondominioId)
    : IRequest<IEnumerable<EncomendaDto>>;

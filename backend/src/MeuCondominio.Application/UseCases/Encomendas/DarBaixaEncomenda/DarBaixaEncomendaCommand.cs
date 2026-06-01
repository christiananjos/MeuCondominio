using MediatR;

namespace MeuCondominio.Application.UseCases.Encomendas.DarBaixaEncomenda;

public sealed record DarBaixaEncomendaCommand(
    Guid   CondominioId,
    Guid   EncomendaId,
    string CodigoInformado
) : IRequest<DarBaixaEncomendaResponse>;

public sealed record DarBaixaEncomendaResponse(
    Guid     EncomendaId,
    string   Status,
    DateTime DataRetirada
);

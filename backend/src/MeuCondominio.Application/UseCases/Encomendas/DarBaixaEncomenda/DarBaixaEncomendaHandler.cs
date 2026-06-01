using MediatR;
using MeuCondominio.Domain.Interfaces.Repositories;

namespace MeuCondominio.Application.UseCases.Encomendas.DarBaixaEncomenda;

public sealed class DarBaixaEncomendaHandler
    : IRequestHandler<DarBaixaEncomendaCommand, DarBaixaEncomendaResponse>
{
    private readonly IEncomendaRepository _encomendaRepo;

    public DarBaixaEncomendaHandler(IEncomendaRepository encomendaRepo)
        => _encomendaRepo = encomendaRepo;

    public async Task<DarBaixaEncomendaResponse> Handle(
        DarBaixaEncomendaCommand command,
        CancellationToken        cancellationToken)
    {
        var encomenda = await _encomendaRepo.ObterPorIdAsync(
            command.EncomendaId, command.CondominioId, cancellationToken)
            ?? throw new InvalidOperationException("Encomenda não encontrada.");

        // Reconstitui o estado do domínio (já que veio do banco "cru")
        encomenda.RecarregarEstado();

        // A validação do código e a transição de estado ficam no DOMÍNIO.
        // DomainException é lançada aqui se o código estiver errado.
        encomenda.Retirar(command.CodigoInformado);

        await _encomendaRepo.AtualizarAsync(encomenda, cancellationToken);

        return new DarBaixaEncomendaResponse(
            encomenda.Id,
            encomenda.Status,
            encomenda.DataRetirada!.Value);
    }
}

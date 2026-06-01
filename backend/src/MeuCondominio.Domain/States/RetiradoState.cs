using MeuCondominio.Domain.Entities;
using MeuCondominio.Domain.Enums;
using MeuCondominio.Domain.Exceptions;

namespace MeuCondominio.Domain.States;

/// <summary>
/// Estado terminal: encomenda retirada pelo morador.
/// Nenhuma transição adicional é permitida.
/// </summary>
public sealed class RetiradoState : IEncomendaState
{
    public string NomeStatus => StatusEncomenda.Retirada;

    public void Notificar(Encomenda encomenda)
        => throw new DomainException("Esta encomenda já foi retirada.");

    public void Retirar(Encomenda encomenda, string codigoInformado)
        => throw new DomainException("Esta encomenda já foi retirada.");

    public void Extraviar(Encomenda encomenda)
        => throw new DomainException("Não é possível extraviar uma encomenda que já foi retirada.");
}

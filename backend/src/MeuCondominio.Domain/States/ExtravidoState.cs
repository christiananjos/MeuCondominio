using MeuCondominio.Domain.Entities;
using MeuCondominio.Domain.Enums;
using MeuCondominio.Domain.Exceptions;

namespace MeuCondominio.Domain.States;

/// <summary>
/// Estado terminal: encomenda marcada como extraviada.
/// </summary>
public sealed class ExtravidoState : IEncomendaState
{
    public string NomeStatus => StatusEncomenda.Extraviado;

    public void Notificar(Encomenda encomenda)
        => throw new DomainException("Encomenda extraviada não pode ser notificada.");

    public void Retirar(Encomenda encomenda, string codigoInformado)
        => throw new DomainException("Encomenda extraviada não pode ser dada como retirada.");

    public void Extraviar(Encomenda encomenda)
        => throw new DomainException("Encomenda já está marcada como extraviada.");
}

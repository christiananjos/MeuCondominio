using MeuCondominio.Domain.Entities;
using MeuCondominio.Domain.Enums;
using MeuCondominio.Domain.Exceptions;

namespace MeuCondominio.Domain.States;

/// <summary>
/// Estado inicial da encomenda. Transições permitidas: Notificar, Extraviar.
/// </summary>
public sealed class PendenteState : IEncomendaState
{
    public string NomeStatus => StatusEncomenda.Pendente;

    public void Notificar(Encomenda encomenda)
        => encomenda.TransicionarPara(new NotificadoState());

    public void Retirar(Encomenda encomenda, string codigoInformado)
        => throw new DomainException("Encomenda ainda não foi notificada ao morador. Chame Notificar() primeiro.");

    public void Extraviar(Encomenda encomenda)
        => encomenda.TransicionarPara(new ExtravidoState());
}

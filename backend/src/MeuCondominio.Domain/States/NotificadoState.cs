using MeuCondominio.Domain.Entities;
using MeuCondominio.Domain.Enums;
using MeuCondominio.Domain.Exceptions;

namespace MeuCondominio.Domain.States;

/// <summary>
/// Morador foi notificado via WhatsApp. Transições permitidas: Retirar, Extraviar.
/// </summary>
public sealed class NotificadoState : IEncomendaState
{
    public string NomeStatus => StatusEncomenda.Notificado;

    public void Notificar(Encomenda encomenda)
        => throw new DomainException("Morador já foi notificado para esta encomenda.");

    public void Retirar(Encomenda encomenda, string codigoInformado)
    {
        if (string.IsNullOrWhiteSpace(codigoInformado))
            throw new DomainException("O código de retirada não pode ser vazio.");

        if (!encomenda.CodigoRetirada.Equals(codigoInformado.Trim(), StringComparison.Ordinal))
            throw new DomainException("Código de retirada inválido. Verifique e tente novamente.");

        encomenda.DefinirDataRetirada(DateTime.UtcNow);
        encomenda.TransicionarPara(new RetiradoState());
    }

    public void Extraviar(Encomenda encomenda)
        => encomenda.TransicionarPara(new ExtravidoState());
}

using MeuCondominio.Domain.Entities;

namespace MeuCondominio.Domain.States;

/// <summary>
/// Contrato do padrão STATE para o ciclo de vida da Encomenda.
/// Cada estado encapsula o que é permitido e o que acontece nas transições.
/// </summary>
public interface IEncomendaState
{
    string NomeStatus { get; }
    void Notificar(Encomenda encomenda);
    void Retirar(Encomenda encomenda, string codigoInformado);
    void Extraviar(Encomenda encomenda);
}

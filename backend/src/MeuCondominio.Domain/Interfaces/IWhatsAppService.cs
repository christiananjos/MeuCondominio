namespace MeuCondominio.Domain.Interfaces;

/// <summary>
/// Contrato para envio de notificações via WhatsApp.
/// A implementação concreta (MetaWhatsAppService) fica na camada de Infrastructure.
/// </summary>
public interface IWhatsAppService
{
    /// <summary>
    /// Envia uma notificação de encomenda recebida para o morador.
    /// Os tokens de API são buscados dinamicamente por condominioId (multi-tenant).
    /// </summary>
    Task<bool> EnviarNotificacaoEncomendaAsync(
        Guid              condominioId,
        string            destinatarioWhatsApp,
        string            nomeMorador,
        string            tipoEncomenda,
        string            codigoRetirada,
        CancellationToken cancellationToken = default);
}

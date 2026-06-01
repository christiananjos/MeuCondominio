using MediatR;

namespace MeuCondominio.Application.UseCases.Encomendas.RegistrarEncomenda;

/// <summary>
/// Command CQRS para registrar uma nova encomenda na portaria.
/// Carrega os dados já validados pelo OCR (ou preenchidos manualmente).
/// </summary>
public sealed record RegistrarEncomendaCommand(
    Guid   CondominioId,
    Guid   MoradorId,
    string TipoEncomenda,
    string? TextoOcrBruto   // Nulo quando preenchido manualmente
) : IRequest<RegistrarEncomendaResponse>;

public sealed record RegistrarEncomendaResponse(
    Guid   EncomendaId,
    string CodigoRetirada,
    bool   WhatsAppEnviado
);

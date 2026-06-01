namespace MeuCondominio.Application.DTOs;

/// <summary>
/// DTO de saída para encomendas. Nunca expõe o código de retirada
/// ou o número completo do WhatsApp na listagem.
/// </summary>
public sealed record EncomendaDto(
    Guid      Id,
    Guid      MoradorId,
    string    NomeMorador,
    string    Bloco,
    string    Apartamento,
    string    TipoEncomenda,
    string    Status,
    DateTime  DataRecebimento,
    DateTime? DataRetirada
);

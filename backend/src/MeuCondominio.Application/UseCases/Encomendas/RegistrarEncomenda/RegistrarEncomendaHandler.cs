using MediatR;
using MeuCondominio.Domain.Entities;
using MeuCondominio.Domain.Interfaces;
using MeuCondominio.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MeuCondominio.Application.UseCases.Encomendas.RegistrarEncomenda;

/// <summary>
/// Handler CQRS: orquestra a criação da encomenda, persistência e
/// disparo da notificação WhatsApp de forma assíncrona.
/// </summary>
public sealed class RegistrarEncomendaHandler
    : IRequestHandler<RegistrarEncomendaCommand, RegistrarEncomendaResponse>
{
    private readonly IEncomendaRepository _encomendaRepo;
    private readonly IMoradorRepository   _moradorRepo;
    private readonly IWhatsAppService     _whatsAppService;
    private readonly ILogger<RegistrarEncomendaHandler> _logger;

    public RegistrarEncomendaHandler(
        IEncomendaRepository            encomendaRepo,
        IMoradorRepository              moradorRepo,
        IWhatsAppService                whatsAppService,
        ILogger<RegistrarEncomendaHandler> logger)
    {
        _encomendaRepo   = encomendaRepo;
        _moradorRepo     = moradorRepo;
        _whatsAppService = whatsAppService;
        _logger          = logger;
    }

    public async Task<RegistrarEncomendaResponse> Handle(
        RegistrarEncomendaCommand command,
        CancellationToken         cancellationToken)
    {
        // 1. Valida morador (pertence ao mesmo condomínio — RLS cobre no DB)
        var morador = await _moradorRepo.ObterPorIdAsync(
            command.MoradorId, command.CondominioId, cancellationToken)
            ?? throw new InvalidOperationException($"Morador {command.MoradorId} não encontrado.");

        // 2. Cria a entidade de domínio (gera código seguro internamente)
        var encomenda = Encomenda.Criar(command.CondominioId, command.MoradorId, command.TipoEncomenda);

        // 3. Persiste
        await _encomendaRepo.AdicionarAsync(encomenda, cancellationToken);

        // 4. Avança o estado para Notificado e dispara WhatsApp
        encomenda.Notificar();

        var enviado = await _whatsAppService.EnviarNotificacaoEncomendaAsync(
            condominioId:          command.CondominioId,
            destinatarioWhatsApp:  morador.WhatsApp,
            nomeMorador:           morador.NomeResponsavel,
            tipoEncomenda:         command.TipoEncomenda,
            codigoRetirada:        encomenda.CodigoRetirada,
            cancellationToken:     cancellationToken);

        if (!enviado)
            _logger.LogWarning("WhatsApp não enviado para morador {MoradorId}.", command.MoradorId);

        // 5. Persiste o novo status (Notificado)
        await _encomendaRepo.AtualizarAsync(encomenda, cancellationToken);

        return new RegistrarEncomendaResponse(encomenda.Id, encomenda.CodigoRetirada, enviado);
    }
}

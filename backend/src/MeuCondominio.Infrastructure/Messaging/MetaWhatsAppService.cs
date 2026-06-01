using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MeuCondominio.Domain.Interfaces;
using MeuCondominio.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MeuCondominio.Infrastructure.Messaging;

/// <summary>
/// Implementação concreta de IWhatsAppService usando a API Cloud Oficial da Meta.
/// Endpoint: POST https://graph.facebook.com/v17.0/{phone_number_id}/messages
///
/// Multi-tenant: os tokens são buscados no banco por condominio_id —
/// nenhuma credencial fica hardcoded ou no appsettings.
/// </summary>
public sealed class MetaWhatsAppService : IWhatsAppService
{
    private const string MetaApiVersion = "v17.0";
    private const string HttpClientName = "MetaWhatsApp";

    private readonly IHttpClientFactory        _httpClientFactory;
    private readonly IWhatsAppConfigRepository _configRepository;
    private readonly ILogger<MetaWhatsAppService> _logger;

    public MetaWhatsAppService(
        IHttpClientFactory           httpClientFactory,
        IWhatsAppConfigRepository    configRepository,
        ILogger<MetaWhatsAppService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configRepository  = configRepository;
        _logger            = logger;
    }

    public async Task<bool> EnviarNotificacaoEncomendaAsync(
        Guid              condominioId,
        string            destinatarioWhatsApp,
        string            nomeMorador,
        string            tipoEncomenda,
        string            codigoRetirada,
        CancellationToken cancellationToken = default)
    {
        // 1. Obtém config específica do tenant (nunca hardcoded)
        var config = await _configRepository.ObterPorCondominioAsync(condominioId, cancellationToken);
        if (config is null)
        {
            _logger.LogWarning(
                "Configuração WhatsApp não encontrada para o condomínio {CondominioId}. Notificação não enviada.",
                condominioId);
            return false;
        }

        // 2. Monta o payload conforme a especificação da Meta para Template Messages
        var payload = new
        {
            messaging_product = "whatsapp",
            to                = SanitizarNumero(destinatarioWhatsApp),
            type              = "template",
            template = new
            {
                name     = config.TemplateName,
                language = new { code = "pt_BR" },
                components = new[]
                {
                    new
                    {
                        type       = "body",
                        parameters = new object[]
                        {
                            new { type = "text", text = nomeMorador    },
                            new { type = "text", text = tipoEncomenda  },
                            new { type = "text", text = codigoRetirada }
                        }
                    }
                }
            }
        };

        // 3. Disparo HTTP com token do banco (por tenant)
        var url    = $"https://graph.facebook.com/{MetaApiVersion}/{config.PhoneNumberId}/messages";
        var client = _httpClientFactory.CreateClient(HttpClientName);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", config.MetaToken);

        using var response = await client.PostAsJsonAsync(url, payload, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(
                "WhatsApp enviado com sucesso para {Numero} | Condomínio {CondominioId}",
                MascararNumero(destinatarioWhatsApp), condominioId);
            return true;
        }

        // 4. Log detalhado em caso de falha (sem expor o token)
        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError(
            "Falha ao enviar WhatsApp. Status: {StatusCode} | Condomínio: {CondominioId} | Erro: {Erro}",
            (int)response.StatusCode, condominioId, errorBody);

        return false;
    }

    /// <summary>Garante que o número esteja no formato E.164 sem o "+".</summary>
    private static string SanitizarNumero(string numero)
        => numero.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

    /// <summary>Mascara o número para log seguro: 55119****1234</summary>
    private static string MascararNumero(string numero)
    {
        var n = SanitizarNumero(numero);
        return n.Length >= 8
            ? string.Concat(n.AsSpan(0, 4), "****", n.AsSpan(n.Length - 4))
            : "****";
    }
}

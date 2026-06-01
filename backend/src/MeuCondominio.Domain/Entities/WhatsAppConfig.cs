namespace MeuCondominio.Domain.Entities;

public class WhatsAppConfig
{
    public Guid     Id            { get; private set; }
    public Guid     CondominioId  { get; private set; }
    public string   MetaToken     { get; private set; } = string.Empty;
    public string   PhoneNumberId { get; private set; } = string.Empty;
    public string   TemplateName  { get; private set; } = string.Empty;
    public DateTime UpdatedAt     { get; private set; }

    private WhatsAppConfig() { }

    public static WhatsAppConfig Criar(
        Guid   condominioId,
        string metaToken,
        string phoneNumberId,
        string templateName)
        => new()
        {
            Id            = Guid.NewGuid(),
            CondominioId  = condominioId,
            MetaToken     = metaToken,
            PhoneNumberId = phoneNumberId,
            TemplateName  = templateName,
            UpdatedAt     = DateTime.UtcNow
        };

    public void Atualizar(string metaToken, string phoneNumberId, string templateName)
    {
        MetaToken     = metaToken;
        PhoneNumberId = phoneNumberId;
        TemplateName  = templateName;
        UpdatedAt     = DateTime.UtcNow;
    }
}

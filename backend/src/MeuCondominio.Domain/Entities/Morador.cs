namespace MeuCondominio.Domain.Entities;

public class Morador
{
    public Guid     Id               { get; private set; }
    public Guid     CondominioId     { get; private set; }
    public string   Bloco            { get; private set; } = string.Empty;
    public string   Apartamento      { get; private set; } = string.Empty;
    public string   NomeResponsavel  { get; private set; } = string.Empty;
    public string   WhatsApp         { get; private set; } = string.Empty;
    public DateTime CreatedAt        { get; private set; }

    private Morador() { }

    public static Morador Criar(
        Guid   condominioId,
        string bloco,
        string apartamento,
        string nomeResponsavel,
        string whatsApp)
    {
        return new Morador
        {
            Id              = Guid.NewGuid(),
            CondominioId    = condominioId,
            Bloco           = bloco.Trim().ToUpper(),
            Apartamento     = apartamento.Trim().ToUpper(),
            NomeResponsavel = nomeResponsavel.Trim(),
            WhatsApp        = whatsApp.Trim(),
            CreatedAt       = DateTime.UtcNow
        };
    }

    public void Atualizar(string nomeResponsavel, string whatsApp)
    {
        NomeResponsavel = nomeResponsavel.Trim();
        WhatsApp        = whatsApp.Trim();
    }
}

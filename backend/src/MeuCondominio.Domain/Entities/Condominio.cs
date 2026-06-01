namespace MeuCondominio.Domain.Entities;

public class Condominio
{
    public Guid     Id             { get; private set; }
    public string   NomeComercial  { get; private set; } = string.Empty;
    public string?  Cnpj           { get; private set; }
    public bool     Ativo          { get; private set; }
    public DateTime CreatedAt      { get; private set; }

    private Condominio() { }

    public static Condominio Criar(string nomeComercial, string? cnpj = null)
        => new()
        {
            Id            = Guid.NewGuid(),
            NomeComercial = nomeComercial.Trim(),
            Cnpj          = cnpj?.Trim(),
            Ativo         = true,
            CreatedAt     = DateTime.UtcNow
        };

    public void Desativar() => Ativo = false;
    public void Ativar()    => Ativo = true;
}

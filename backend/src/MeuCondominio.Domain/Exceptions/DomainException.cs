namespace MeuCondominio.Domain.Exceptions;

/// <summary>
/// Representa uma violação de regra de negócio no domínio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

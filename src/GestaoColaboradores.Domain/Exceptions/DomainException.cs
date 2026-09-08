namespace GestaoColaboradores.Domain.Exceptions;

/// <summary>
/// Invariante de domínio violada. Tipo próprio para que o middleware
/// distinga erro de negócio (400) de falha inesperada (500).
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string mensagem) : base(mensagem)
    {
    }

    public DomainException(string mensagem, Exception innerException)
        : base(mensagem, innerException)
    {
    }
}

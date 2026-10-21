namespace FCG.Users.Domain.Exceptions;

public sealed class EmailInvalidoException : DomainException
{
    public EmailInvalidoException(string mensagem) : base(mensagem)
    {
    }
}

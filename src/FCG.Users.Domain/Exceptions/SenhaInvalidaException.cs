namespace FCG.Users.Domain.Exceptions;

public sealed class SenhaInvalidaException : DomainException
{
    public SenhaInvalidaException(string mensagem) : base(mensagem)
    {
    }
}

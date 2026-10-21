namespace FCG.Users.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string mensagem) : base(mensagem)
    {
    }
}

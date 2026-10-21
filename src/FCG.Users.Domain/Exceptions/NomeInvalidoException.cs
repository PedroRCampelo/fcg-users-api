namespace FCG.Users.Domain.Exceptions;

public sealed class NomeInvalidoException : DomainException
{
    public NomeInvalidoException(string mensagem) : base(mensagem)
    {
    }
}

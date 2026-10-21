namespace FCG.Users.Application.Exceptions;

public sealed class CredenciaisInvalidasException : Exception
{
    public CredenciaisInvalidasException(string mensagem) : base(mensagem)
    {
    }
}

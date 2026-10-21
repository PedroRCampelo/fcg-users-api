using System.Text.RegularExpressions;
using FCG.Users.Domain.Exceptions;

namespace FCG.Users.Domain.ValueObjects;

public sealed record Email
{
    private static readonly Regex FormatoValido = new(
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+$",
        RegexOptions.Compiled);

    public string Endereco { get; }

    public Email(string endereco)
    {
        if (string.IsNullOrWhiteSpace(endereco) || !FormatoValido.IsMatch(endereco))
            throw new EmailInvalidoException($"O e-mail '{endereco}' é inválido.");

        Endereco = endereco.Trim().ToLowerInvariant();
    }

    public override string ToString() => Endereco;
}

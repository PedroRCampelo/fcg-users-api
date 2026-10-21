using FCG.Users.Domain.Exceptions;

namespace FCG.Users.Domain.ValueObjects;

public sealed record Senha
{
    private const int TamanhoMinimo = 8;

    public string Valor { get; }

    public Senha(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new SenhaInvalidaException("A senha não pode ser vazia.");

        if (valor.Length < TamanhoMinimo)
            throw new SenhaInvalidaException($"A senha deve ter no mínimo {TamanhoMinimo} caracteres.");

        if (!valor.Any(char.IsLetter))
            throw new SenhaInvalidaException("A senha deve conter ao menos uma letra.");

        if (!valor.Any(char.IsDigit))
            throw new SenhaInvalidaException("A senha deve conter ao menos um número.");

        if (valor.All(char.IsLetterOrDigit))
            throw new SenhaInvalidaException("A senha deve conter ao menos um caractere especial.");

        Valor = valor;
    }

    public override string ToString() => "******";
}

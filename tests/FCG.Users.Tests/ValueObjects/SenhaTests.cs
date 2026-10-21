using FCG.Users.Domain.Exceptions;
using FCG.Users.Domain.ValueObjects;
using Xunit;

namespace FCG.Users.Tests.ValueObjects;

public class SenhaTests
{
    [Theory]
    [InlineData("Senha123!")]
    [InlineData("Abcdefg1@")]
    [InlineData("C0mplex#Pass")]
    public void Deve_Criar_Senha_Quando_Valor_For_Valido(string valor)
    {
        var senha = new Senha(valor);

        Assert.Equal(valor, senha.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Valor_For_Nulo_Ou_Vazio(string? valor)
    {
        Assert.Throws<SenhaInvalidaException>(() => new Senha(valor!));
    }

    [Theory]
    [InlineData("Ab1!")]
    [InlineData("Curta1!")]
    public void Deve_Lancar_Excecao_Quando_Tiver_Menos_De_Oito_Caracteres(string valor)
    {
        Assert.Throws<SenhaInvalidaException>(() => new Senha(valor));
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Nao_Tiver_Letra()
    {
        Assert.Throws<SenhaInvalidaException>(() => new Senha("12345678!"));
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Nao_Tiver_Numero()
    {
        Assert.Throws<SenhaInvalidaException>(() => new Senha("Senhaaaa!"));
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Nao_Tiver_Caractere_Especial()
    {
        Assert.Throws<SenhaInvalidaException>(() => new Senha("Senha1234"));
    }

    [Fact]
    public void ToString_Nao_Deve_Expor_O_Valor_Da_Senha()
    {
        var senha = new Senha("Senha123!");

        Assert.DoesNotContain("Senha123!", senha.ToString());
    }

    [Fact]
    public void Duas_Senhas_Com_Mesmo_Valor_Devem_Ser_Iguais()
    {
        var senha1 = new Senha("Senha123!");
        var senha2 = new Senha("Senha123!");

        Assert.Equal(senha1, senha2);
    }
}

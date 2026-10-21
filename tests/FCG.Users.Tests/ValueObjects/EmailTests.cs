using FCG.Users.Domain.Exceptions;
using FCG.Users.Domain.ValueObjects;
using Xunit;

namespace FCG.Users.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("usuario@dominio.com")]
    [InlineData("nome.sobrenome@empresa.com.br")]
    [InlineData("usuario+tag@sub.dominio.io")]
    [InlineData("USUARIO@DOMINIO.COM")]
    public void Deve_Criar_Email_Quando_Endereco_For_Valido(string endereco)
    {
        var email = new Email(endereco);

        Assert.Equal(endereco.ToLowerInvariant(), email.Endereco);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sem-arroba.com")]
    [InlineData("@dominio.com")]
    [InlineData("usuario@")]
    [InlineData("usuario@dominio")]
    [InlineData("usuario dominio.com")]
    [InlineData("usuario@@dominio.com")]
    public void Deve_Lancar_Excecao_Quando_Endereco_For_Invalido(string? endereco)
    {
        Assert.Throws<EmailInvalidoException>(() => new Email(endereco!));
    }

    [Fact]
    public void Dois_Emails_Com_Mesmo_Endereco_Devem_Ser_Iguais()
    {
        var email1 = new Email("usuario@dominio.com");
        var email2 = new Email("USUARIO@DOMINIO.COM");

        Assert.Equal(email1, email2);
        Assert.True(email1 == email2);
    }

    [Fact]
    public void ToString_Deve_Retornar_Endereco()
    {
        var email = new Email("usuario@dominio.com");

        Assert.Equal("usuario@dominio.com", email.ToString());
    }
}

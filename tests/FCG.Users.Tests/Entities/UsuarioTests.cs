using FCG.Users.Domain.Entities;
using FCG.Users.Domain.Enums;
using FCG.Users.Domain.Exceptions;
using FCG.Users.Domain.ValueObjects;
using Xunit;

namespace FCG.Users.Tests.Entities;

public class UsuarioTests
{
    private static Usuario CriarUsuario(string nome = "Jogador Um", string email = "jogador@dominio.com")
        => new(nome, new Email(email), "hash-fake-de-senha");

    [Fact]
    public void Deve_Criar_Usuario_Com_Role_Usuario_E_Ativo_Por_Padrao()
    {
        var usuario = CriarUsuario();

        Assert.Equal(RoleUsuario.Usuario, usuario.Role);
        Assert.True(usuario.Ativo);
        Assert.NotEqual(Guid.Empty, usuario.Id);
    }

    [Fact]
    public void Deve_Remover_Espacos_Do_Nome()
    {
        var usuario = CriarUsuario(nome: "  Jogador Um  ");

        Assert.Equal("Jogador Um", usuario.Nome);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Nome_For_Invalido(string? nome)
    {
        Assert.Throws<NomeInvalidoException>(() => new Usuario(nome!, new Email("jogador@dominio.com"), "hash"));
    }

    [Fact]
    public void Dois_Usuarios_Devem_Ter_Ids_Diferentes()
    {
        Assert.NotEqual(CriarUsuario().Id, CriarUsuario().Id);
    }

    [Fact]
    public void Promover_Deve_Alterar_Role_Para_Administrador()
    {
        var usuario = CriarUsuario();

        usuario.Promover();

        Assert.Equal(RoleUsuario.Administrador, usuario.Role);
    }

    [Fact]
    public void Desativar_Deve_Alterar_Ativo_Para_Falso()
    {
        var usuario = CriarUsuario();

        usuario.Desativar();

        Assert.False(usuario.Ativo);
    }
}

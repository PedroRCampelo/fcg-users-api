using FCG.Users.Application.Exceptions;
using FCG.Users.Application.Services;
using FCG.Users.Domain.Entities;
using FCG.Users.Domain.Enums;
using FCG.Users.Domain.ValueObjects;
using FCG.Users.Tests.Fakes;
using Xunit;

namespace FCG.Users.Tests.Services;

public class UsuarioServiceTests
{
    private readonly FakeUsuarioRepository _usuarios = new();
    private readonly FakeUnitOfWork _unitOfWork = new(new List<string>());
    private readonly UsuarioService _servico;

    public UsuarioServiceTests()
    {
        _servico = new UsuarioService(_usuarios, _unitOfWork);
    }

    private Usuario AdicionarUsuario(string email = "jogador@dominio.com")
    {
        var usuario = new Usuario("Jogador Um", new Email(email), "hash");
        _usuarios.Usuarios.Add(usuario);
        return usuario;
    }

    [Fact]
    public async Task Listar_Deve_Retornar_Todos_Os_Usuarios_Sem_Expor_A_Senha()
    {
        AdicionarUsuario("um@dominio.com");
        AdicionarUsuario("dois@dominio.com");

        var resposta = await _servico.ListarAsync();

        Assert.Equal(2, resposta.Count);
        Assert.DoesNotContain(typeof(FCG.Users.Application.Dtos.UsuarioResponse).GetProperties(), p => p.Name.Contains("Senha"));
    }

    [Fact]
    public async Task Promover_Deve_Tornar_O_Usuario_Administrador_E_Salvar()
    {
        var usuario = AdicionarUsuario();

        await _servico.PromoverAsync(usuario.Id);

        Assert.Equal(RoleUsuario.Administrador, usuario.Role);
        Assert.Equal(1, _unitOfWork.Salvamentos);
    }

    [Fact]
    public async Task Promover_Usuario_Inexistente_Deve_Lancar_Nao_Encontrado()
    {
        await Assert.ThrowsAsync<NaoEncontradoException>(() => _servico.PromoverAsync(Guid.NewGuid()));

        Assert.Equal(0, _unitOfWork.Salvamentos);
    }

    [Fact]
    public async Task Desativar_Deve_Marcar_O_Usuario_Como_Inativo_E_Salvar()
    {
        var usuario = AdicionarUsuario();

        await _servico.DesativarAsync(usuario.Id);

        Assert.False(usuario.Ativo);
        Assert.Equal(1, _unitOfWork.Salvamentos);
    }

    [Fact]
    public async Task Desativar_Usuario_Inexistente_Deve_Lancar_Nao_Encontrado()
    {
        await Assert.ThrowsAsync<NaoEncontradoException>(() => _servico.DesativarAsync(Guid.NewGuid()));
    }
}

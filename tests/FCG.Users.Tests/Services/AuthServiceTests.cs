using FCG.Contracts;
using FCG.Users.Application.Dtos;
using FCG.Users.Application.Exceptions;
using FCG.Users.Application.Services;
using FCG.Users.Domain.Exceptions;
using FCG.Users.Tests.Fakes;
using Xunit;

namespace FCG.Users.Tests.Services;

public class AuthServiceTests
{
    private readonly List<string> _chamadas = new();
    private readonly FakeUsuarioRepository _usuarios = new();
    private readonly FakeEventPublisher _eventos;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly AuthService _servico;

    public AuthServiceTests()
    {
        _eventos = new FakeEventPublisher(_chamadas);
        _unitOfWork = new FakeUnitOfWork(_chamadas);
        _servico = new AuthService(_usuarios, _unitOfWork, _eventos, new FakePasswordHasher(), new FakeTokenService());
    }

    private static RegistrarUsuarioRequest NovoRegistro(
        string nome = "Jogador Um",
        string email = "jogador@dominio.com",
        string senha = "Senha123!") => new(nome, email, senha);

    [Fact]
    public async Task Registrar_Deve_Criar_Usuario_Com_Role_Usuario_E_Ativo()
    {
        var resposta = await _servico.RegistrarAsync(NovoRegistro());

        Assert.Equal("Usuario", resposta.Role);
        Assert.True(resposta.Ativo);
        Assert.Equal("jogador@dominio.com", resposta.Email);
        Assert.Single(_usuarios.Usuarios);
    }

    [Fact]
    public async Task Registrar_Nao_Deve_Guardar_A_Senha_Em_Texto_Puro()
    {
        await _servico.RegistrarAsync(NovoRegistro(senha: "Senha123!"));

        var salvo = Assert.Single(_usuarios.Usuarios);
        Assert.NotEqual("Senha123!", salvo.SenhaHash);
        Assert.Equal("hash:Senha123!", salvo.SenhaHash);
    }

    [Fact]
    public async Task Registrar_Deve_Publicar_UserCreatedEvent_Com_Os_Dados_Do_Usuario()
    {
        var resposta = await _servico.RegistrarAsync(NovoRegistro(nome: "Jogador Um", email: "Jogador@Dominio.com"));

        var evento = Assert.IsType<UserCreatedEvent>(Assert.Single(_eventos.Publicados));
        Assert.Equal(resposta.Id, evento.UserId);
        Assert.Equal("Jogador Um", evento.Nome);
        Assert.Equal("jogador@dominio.com", evento.Email);
    }

    [Fact]
    public async Task Registrar_Deve_Registrar_O_Evento_Antes_De_Salvar_Para_Entrar_Na_Mesma_Transacao()
    {
        await _servico.RegistrarAsync(NovoRegistro());

        Assert.Equal(new[] { "publicar", "salvar" }, _chamadas);
        Assert.Equal(1, _unitOfWork.Salvamentos);
    }

    [Fact]
    public async Task Registrar_Com_Email_Ja_Cadastrado_Deve_Lancar_Conflito_Sem_Publicar_Nem_Salvar()
    {
        await _servico.RegistrarAsync(NovoRegistro());
        _eventos.Publicados.Clear();
        _chamadas.Clear();

        await Assert.ThrowsAsync<ConflitoException>(() => _servico.RegistrarAsync(NovoRegistro(nome: "Outro")));

        Assert.Empty(_eventos.Publicados);
        Assert.Empty(_chamadas);
        Assert.Single(_usuarios.Usuarios);
    }

    [Fact]
    public async Task Registrar_Com_Email_Duplicado_Em_Caixa_Diferente_Tambem_Deve_Lancar_Conflito()
    {
        await _servico.RegistrarAsync(NovoRegistro(email: "jogador@dominio.com"));

        await Assert.ThrowsAsync<ConflitoException>(() => _servico.RegistrarAsync(NovoRegistro(email: "JOGADOR@DOMINIO.COM")));
    }

    [Theory]
    [InlineData("curta1!")]
    [InlineData("semnumero!")]
    [InlineData("12345678!")]
    [InlineData("SemEspecial1")]
    public async Task Registrar_Com_Senha_Fora_Das_Regras_Nao_Deve_Persistir_Nem_Publicar(string senha)
    {
        await Assert.ThrowsAsync<SenhaInvalidaException>(() => _servico.RegistrarAsync(NovoRegistro(senha: senha)));

        Assert.Empty(_usuarios.Usuarios);
        Assert.Empty(_eventos.Publicados);
        Assert.Empty(_chamadas);
    }

    [Fact]
    public async Task Registrar_Com_Email_Invalido_Nao_Deve_Persistir_Nem_Publicar()
    {
        await Assert.ThrowsAsync<EmailInvalidoException>(() => _servico.RegistrarAsync(NovoRegistro(email: "nao-e-email")));

        Assert.Empty(_usuarios.Usuarios);
        Assert.Empty(_eventos.Publicados);
    }

    [Fact]
    public async Task Registrar_Com_Nome_Vazio_Nao_Deve_Publicar()
    {
        await Assert.ThrowsAsync<NomeInvalidoException>(() => _servico.RegistrarAsync(NovoRegistro(nome: "  ")));

        Assert.Empty(_eventos.Publicados);
        Assert.Empty(_chamadas);
    }

    [Fact]
    public async Task Login_Com_Credenciais_Corretas_Deve_Retornar_Token()
    {
        var usuario = await _servico.RegistrarAsync(NovoRegistro());

        var token = await _servico.LoginAsync(new LoginRequest("jogador@dominio.com", "Senha123!"));

        Assert.Equal($"token-de-{usuario.Id}", token.Token);
    }

    [Fact]
    public async Task Login_Com_Senha_Errada_Deve_Lancar_Credenciais_Invalidas()
    {
        await _servico.RegistrarAsync(NovoRegistro());

        await Assert.ThrowsAsync<CredenciaisInvalidasException>(
            () => _servico.LoginAsync(new LoginRequest("jogador@dominio.com", "SenhaErrada1!")));
    }

    [Fact]
    public async Task Login_Com_Email_Inexistente_Deve_Lancar_Credenciais_Invalidas()
    {
        await Assert.ThrowsAsync<CredenciaisInvalidasException>(
            () => _servico.LoginAsync(new LoginRequest("ninguem@dominio.com", "Senha123!")));
    }

    [Fact]
    public async Task Login_De_Usuario_Desativado_Deve_Lancar_Credenciais_Invalidas()
    {
        await _servico.RegistrarAsync(NovoRegistro());
        _usuarios.Usuarios.Single().Desativar();

        await Assert.ThrowsAsync<CredenciaisInvalidasException>(
            () => _servico.LoginAsync(new LoginRequest("jogador@dominio.com", "Senha123!")));
    }

    [Fact]
    public async Task Login_Nao_Deve_Revelar_Se_O_Email_Existe()
    {
        await _servico.RegistrarAsync(NovoRegistro());

        var senhaErrada = await Assert.ThrowsAsync<CredenciaisInvalidasException>(
            () => _servico.LoginAsync(new LoginRequest("jogador@dominio.com", "SenhaErrada1!")));
        var emailInexistente = await Assert.ThrowsAsync<CredenciaisInvalidasException>(
            () => _servico.LoginAsync(new LoginRequest("ninguem@dominio.com", "Senha123!")));

        Assert.Equal(senhaErrada.Message, emailInexistente.Message);
    }
}

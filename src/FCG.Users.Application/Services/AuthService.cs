using FCG.Contracts;
using FCG.Users.Application.Dtos;
using FCG.Users.Application.Exceptions;
using FCG.Users.Application.Interfaces;
using FCG.Users.Domain.Entities;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.Application.Services;

public class AuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventos;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork,
        IEventPublisher eventos,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
        _eventos = eventos;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<UsuarioResponse> RegistrarAsync(RegistrarUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        var email = new Email(request.Email);
        var senha = new Senha(request.Senha);

        if (await _usuarios.ExisteComEmailAsync(email, cancellationToken))
            throw new ConflitoException("Já existe um usuário cadastrado com este e-mail.");

        var usuario = new Usuario(request.Nome, email, _passwordHasher.Hash(senha.Valor));

        await _usuarios.AdicionarAsync(usuario, cancellationToken);

        // O evento é registrado antes do SalvarAsync de propósito: a infraestrutura grava
        // usuário e evento na mesma transação (outbox), então um nunca existe sem o outro.
        await _eventos.PublicarAsync(new UserCreatedEvent(usuario.Id, usuario.Nome, usuario.Email.Endereco), cancellationToken);
        await _unitOfWork.SalvarAsync(cancellationToken);

        return UsuarioMapper.ParaResponse(usuario);
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = new Email(request.Email);
        var usuario = await _usuarios.ObterPorEmailAsync(email, cancellationToken);

        // Mesma mensagem para e-mail inexistente, conta desativada e senha errada,
        // para não revelar quais e-mails estão cadastrados.
        if (usuario is null || !usuario.Ativo || !_passwordHasher.Verificar(request.Senha, usuario.SenhaHash))
            throw new CredenciaisInvalidasException("E-mail ou senha inválidos.");

        return new TokenResponse(_tokenService.GerarToken(usuario));
    }
}

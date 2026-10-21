using FCG.Users.Domain.Enums;
using FCG.Users.Domain.Exceptions;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.Domain.Entities;

public sealed class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;
    public RoleUsuario Role { get; private set; }
    public bool Ativo { get; private set; }

    private Usuario()
    {
    }

    public Usuario(string nome, Email email, string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new NomeInvalidoException("O nome é obrigatório.");

        Id = Guid.NewGuid();
        Nome = nome.Trim();
        Email = email ?? throw new ArgumentNullException(nameof(email));
        SenhaHash = senhaHash ?? throw new ArgumentNullException(nameof(senhaHash));
        Role = RoleUsuario.Usuario;
        Ativo = true;
    }

    public void Promover() => Role = RoleUsuario.Administrador;

    public void Desativar() => Ativo = false;
}

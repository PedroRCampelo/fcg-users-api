using FCG.Users.Application.Interfaces;
using FCG.Users.Domain.Entities;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.Tests.Fakes;

public class FakeUsuarioRepository : IUsuarioRepository
{
    public List<Usuario> Usuarios { get; } = new();

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));

    public Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Usuarios.FirstOrDefault(u => u.Email == email));

    public Task<bool> ExisteComEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Usuarios.Any(u => u.Email == email));

    public Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Usuario>>(Usuarios.ToList());

    public Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        Usuarios.Add(usuario);
        return Task.CompletedTask;
    }
}

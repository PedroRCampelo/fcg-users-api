using FCG.Users.Domain.Entities;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> ExisteComEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}

using FCG.Users.Application.Interfaces;
using FCG.Users.Domain.Entities;
using FCG.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FCG.Users.Infrastructure.Persistence;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly UsersDbContext _context;

    public UsuarioRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.Endereco == email.Endereco, cancellationToken);

    public async Task<bool> ExisteComEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        await _context.Usuarios.AnyAsync(u => u.Email.Endereco == email.Endereco, cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken = default) =>
        await _context.Usuarios.AsNoTracking().OrderBy(u => u.Nome).ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
}

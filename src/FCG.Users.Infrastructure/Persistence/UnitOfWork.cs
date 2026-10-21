using FCG.Users.Application.Exceptions;
using FCG.Users.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FCG.Users.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly UsersDbContext _context;

    public UnitOfWork(UsersDbContext context)
    {
        _context = context;
    }

    public async Task SalvarAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        // Duas requisições simultâneas com o mesmo e-mail passam juntas pela checagem de
        // existência. O índice único do banco é quem decide, e vira 409 em vez de 500.
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflitoException("Já existe um usuário cadastrado com este e-mail.");
        }
    }
}

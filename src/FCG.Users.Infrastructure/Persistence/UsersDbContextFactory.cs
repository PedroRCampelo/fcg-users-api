using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCG.Users.Infrastructure.Persistence;

// Usada apenas pelo dotnet-ef em tempo de design. Gerar uma migration não abre conexão,
// então a string abaixo é só para o provider saber que o destino é PostgreSQL.
public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql("Host=localhost;Database=fcg_users")
            .Options;

        return new UsersDbContext(options);
    }
}

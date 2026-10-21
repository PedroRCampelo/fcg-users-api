using System.Net.Sockets;
using FCG.Users.Application.Interfaces;
using FCG.Users.Domain.Entities;
using FCG.Users.Domain.Enums;
using FCG.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FCG.Users.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    private const int MaximoDeTentativas = 15;
    private static readonly TimeSpan IntervaloEntreTentativas = TimeSpan.FromSeconds(3);

    public static async Task InicializarAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseInitializer));
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        await MigrarAsync(context, logger, cancellationToken);
        await SemearAdministradorAsync(scope.ServiceProvider, context, logger, cancellationToken);
    }

    // No docker compose e no Kubernetes o banco pode demorar mais que a aplicação para ficar
    // pronto. Falhas de conexão são repetidas; qualquer outro erro de migration falha de imediato.
    private static async Task MigrarAsync(UsersDbContext context, ILogger logger, CancellationToken cancellationToken)
    {
        for (var tentativa = 1; ; tentativa++)
        {
            try
            {
                await context.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Migrations aplicadas no banco de usuários");
                return;
            }
            catch (Exception ex) when (EhFalhaDeConexao(ex) && tentativa < MaximoDeTentativas)
            {
                logger.LogWarning("Banco indisponível (tentativa {Tentativa} de {Maximo}), nova tentativa em {Segundos}s: {Motivo}",
                    tentativa, MaximoDeTentativas, IntervaloEntreTentativas.TotalSeconds, ex.Message);
                await Task.Delay(IntervaloEntreTentativas, cancellationToken);
            }
        }
    }

    private static bool EhFalhaDeConexao(Exception ex) =>
        ex is NpgsqlException { IsTransient: true } or SocketException or TimeoutException
        || ex.InnerException is not null && EhFalhaDeConexao(ex.InnerException);

    private static async Task SemearAdministradorAsync(
        IServiceProvider services, UsersDbContext context, ILogger logger, CancellationToken cancellationToken)
    {
        if (await context.Usuarios.AnyAsync(u => u.Role == RoleUsuario.Administrador, cancellationToken))
            return;

        var configuration = services.GetRequiredService<IConfiguration>();
        var email = configuration["AdminPadrao:Email"];
        var senha = configuration["AdminPadrao:Senha"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            logger.LogWarning("Nenhum administrador existe e 'AdminPadrao' não está configurado: seed ignorado");
            return;
        }

        var hasher = services.GetRequiredService<IPasswordHasher>();
        var admin = new Usuario("Administrador FCG", new Email(email), hasher.Hash(new Senha(senha).Valor));
        admin.Promover();

        context.Usuarios.Add(admin);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Administrador padrão criado: {Email}", admin.Email.Endereco);
        }
        // Outra réplica subindo ao mesmo tempo pode ter criado o administrador primeiro.
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            logger.LogInformation("Administrador padrão já criado por outra instância");
        }
    }
}

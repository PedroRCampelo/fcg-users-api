using FCG.Users.Application.Interfaces;
using FCG.Users.Infrastructure.Messaging;
using FCG.Users.Infrastructure.Persistence;
using FCG.Users.Infrastructure.Security;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string ausente. Defina ConnectionStrings__Default.");

        services.AddDbContext<UsersDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton(JwtSettings.Carregar(configuration));
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        AdicionarMensageria(services, configuration);

        return services;
    }

    private static void AdicionarMensageria(IServiceCollection services, IConfiguration configuration)
    {
        var rabbit = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>() ?? new RabbitMqSettings();

        if (string.IsNullOrWhiteSpace(rabbit.Username) || string.IsNullOrWhiteSpace(rabbit.Password))
            throw new InvalidOperationException("Credenciais do RabbitMQ ausentes. Defina RabbitMq__Username e RabbitMq__Password.");

        services.AddMassTransit(bus =>
        {
            // Os eventos ficam na tabela OutboxMessage, na mesma transação do SaveChanges, e um
            // serviço em background entrega ao RabbitMQ. Se o broker estiver fora do ar no cadastro,
            // o evento espera no banco em vez de se perder (ou de derrubar o cadastro).
            bus.AddEntityFrameworkOutbox<UsersDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
            });

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbit.Host, (ushort)rabbit.Port, rabbit.VirtualHost, host =>
                {
                    host.Username(rabbit.Username);
                    host.Password(rabbit.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    }
}

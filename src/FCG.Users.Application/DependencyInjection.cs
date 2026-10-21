using FCG.Users.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Users.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<UsuarioService>();

        return services;
    }
}

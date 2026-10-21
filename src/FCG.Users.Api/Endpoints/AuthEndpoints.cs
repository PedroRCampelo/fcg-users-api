using FCG.Users.Application.Dtos;
using FCG.Users.Application.Services;

namespace FCG.Users.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/auth").WithTags("Auth");

        grupo.MapPost("/registrar", async (RegistrarUsuarioRequest request, AuthService authService, CancellationToken cancellationToken) =>
            {
                var usuario = await authService.RegistrarAsync(request, cancellationToken);
                return Results.Created($"/usuarios/{usuario.Id}", usuario);
            })
            .WithName("RegistrarUsuario")
            .WithSummary("Registra um novo usuário")
            .WithDescription("Cria um usuário com role \"Usuario\" e publica o evento UserCreatedEvent. Endpoint público.")
            .Produces<UsuarioResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapPost("/login", async (LoginRequest request, AuthService authService, CancellationToken cancellationToken) =>
                Results.Ok(await authService.LoginAsync(request, cancellationToken)))
            .WithName("Login")
            .WithSummary("Autentica um usuário")
            .WithDescription("Valida e-mail e senha e retorna um token JWT. Endpoint público.")
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}

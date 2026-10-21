using FCG.Users.Application.Dtos;
using FCG.Users.Application.Services;
using FCG.Users.Domain.Enums;

namespace FCG.Users.Api.Endpoints;

public static class UsuarioEndpoints
{
    public static IEndpointRouteBuilder MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/usuarios")
            .WithTags("Usuarios")
            .RequireAuthorization(nameof(RoleUsuario.Administrador));

        grupo.MapGet("/", async (UsuarioService usuarioService, CancellationToken cancellationToken) =>
                Results.Ok(await usuarioService.ListarAsync(cancellationToken)))
            .WithName("ListarUsuarios")
            .WithSummary("Lista todos os usuários")
            .WithDescription("Requer role Administrador.")
            .Produces<IReadOnlyList<UsuarioResponse>>(StatusCodes.Status200OK);

        grupo.MapPut("/{id:guid}/promover", async (Guid id, UsuarioService usuarioService, CancellationToken cancellationToken) =>
            {
                await usuarioService.PromoverAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .WithName("PromoverUsuario")
            .WithSummary("Promove um usuário a administrador")
            .WithDescription("Requer role Administrador.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        grupo.MapPut("/{id:guid}/desativar", async (Guid id, UsuarioService usuarioService, CancellationToken cancellationToken) =>
            {
                await usuarioService.DesativarAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .WithName("DesativarUsuario")
            .WithSummary("Desativa um usuário")
            .WithDescription("Requer role Administrador. Um usuário desativado não consegue mais fazer login.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}

using FCG.Users.Application.Dtos;
using FCG.Users.Domain.Entities;

namespace FCG.Users.Application.Services;

internal static class UsuarioMapper
{
    public static UsuarioResponse ParaResponse(Usuario usuario) =>
        new(usuario.Id, usuario.Nome, usuario.Email.Endereco, usuario.Role.ToString(), usuario.Ativo);
}

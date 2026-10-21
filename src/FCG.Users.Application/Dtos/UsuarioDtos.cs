namespace FCG.Users.Application.Dtos;

public record UsuarioResponse(Guid Id, string Nome, string Email, string Role, bool Ativo);

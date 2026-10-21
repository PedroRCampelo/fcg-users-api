namespace FCG.Users.Application.Dtos;

public record RegistrarUsuarioRequest(string Nome, string Email, string Senha);

public record LoginRequest(string Email, string Senha);

public record TokenResponse(string Token);

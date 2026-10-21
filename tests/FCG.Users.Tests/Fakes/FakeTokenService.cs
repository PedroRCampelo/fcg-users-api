using FCG.Users.Application.Interfaces;
using FCG.Users.Domain.Entities;

namespace FCG.Users.Tests.Fakes;

public class FakeTokenService : ITokenService
{
    public string GerarToken(Usuario usuario) => $"token-de-{usuario.Id}";
}

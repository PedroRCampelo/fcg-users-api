using FCG.Users.Domain.Entities;

namespace FCG.Users.Application.Interfaces;

public interface ITokenService
{
    string GerarToken(Usuario usuario);
}

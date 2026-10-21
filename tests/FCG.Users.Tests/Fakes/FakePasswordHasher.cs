using FCG.Users.Application.Interfaces;

namespace FCG.Users.Tests.Fakes;

public class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string senha) => $"hash:{senha}";

    public bool Verificar(string senha, string hash) => hash == Hash(senha);
}

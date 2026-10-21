using FCG.Users.Infrastructure.Security;
using Xunit;

namespace FCG.Users.Tests.Security;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_Deve_Gerar_Valor_Diferente_Da_Senha_Original()
    {
        Assert.NotEqual("Senha123!", _hasher.Hash("Senha123!"));
    }

    [Fact]
    public void Hash_Da_Mesma_Senha_Deve_Gerar_Valores_Diferentes_A_Cada_Chamada()
    {
        Assert.NotEqual(_hasher.Hash("Senha123!"), _hasher.Hash("Senha123!"));
    }

    [Fact]
    public void Verificar_Deve_Retornar_Verdadeiro_Para_Senha_Correta()
    {
        Assert.True(_hasher.Verificar("Senha123!", _hasher.Hash("Senha123!")));
    }

    [Fact]
    public void Verificar_Deve_Retornar_Falso_Para_Senha_Incorreta()
    {
        Assert.False(_hasher.Verificar("SenhaErrada1!", _hasher.Hash("Senha123!")));
    }
}

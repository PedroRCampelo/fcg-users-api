using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FCG.Users.Domain.Entities;
using FCG.Users.Domain.Enums;
using FCG.Users.Domain.ValueObjects;
using FCG.Users.Infrastructure.Security;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace FCG.Users.Tests.Security;

public class JwtTokenServiceTests
{
    private static readonly JwtSettings Settings = new()
    {
        SigningKey = "chave-de-testes-com-tamanho-suficiente-para-hmac-sha256",
        Issuer = "fcg-users-api",
        Audience = "fcg",
        ExpirationMinutes = 30
    };

    private static Usuario CriarUsuario() =>
        new("Jogador Um", new Email("jogador@dominio.com"), "hash");

    private static JwtSecurityToken Ler(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token);

    [Fact]
    public void GerarToken_Deve_Incluir_Issuer_E_Audience_Da_Configuracao()
    {
        var jwt = Ler(new JwtTokenService(Settings).GerarToken(CriarUsuario()));

        Assert.Equal("fcg-users-api", jwt.Issuer);
        Assert.Contains("fcg", jwt.Audiences);
    }

    [Fact]
    public void GerarToken_Deve_Usar_Os_Nomes_Curtos_De_Claim_Combinados_Com_Os_Outros_Servicos()
    {
        var usuario = CriarUsuario();

        var jwt = Ler(new JwtTokenService(Settings).GerarToken(usuario));

        Assert.Equal(usuario.Id.ToString(), jwt.Claims.Single(c => c.Type == "sub").Value);
        Assert.Equal("jogador@dominio.com", jwt.Claims.Single(c => c.Type == "email").Value);
        Assert.Equal("Jogador Um", jwt.Claims.Single(c => c.Type == "name").Value);
        Assert.Equal("Usuario", jwt.Claims.Single(c => c.Type == "role").Value);
    }

    [Fact]
    public void GerarToken_De_Administrador_Deve_Levar_A_Role_Administrador()
    {
        var usuario = CriarUsuario();
        usuario.Promover();

        var jwt = Ler(new JwtTokenService(Settings).GerarToken(usuario));

        Assert.Equal(nameof(RoleUsuario.Administrador), jwt.Claims.Single(c => c.Type == "role").Value);
    }

    [Fact]
    public void GerarToken_Deve_Expirar_De_Acordo_Com_A_Configuracao()
    {
        var jwt = Ler(new JwtTokenService(Settings).GerarToken(CriarUsuario()));

        Assert.InRange(jwt.ValidTo, DateTime.UtcNow.AddMinutes(29), DateTime.UtcNow.AddMinutes(30).AddSeconds(5));
    }

    [Fact]
    public void GerarToken_Deve_Gerar_Um_Jti_Diferente_A_Cada_Chamada()
    {
        var servico = new JwtTokenService(Settings);
        var usuario = CriarUsuario();

        var primeiro = Ler(servico.GerarToken(usuario)).Id;
        var segundo = Ler(servico.GerarToken(usuario)).Id;

        Assert.NotEqual(primeiro, segundo);
    }

    [Fact]
    public async Task Token_Deve_Ser_Valido_Somente_Para_Quem_Tem_A_Mesma_Chave()
    {
        var token = new JwtTokenService(Settings).GerarToken(CriarUsuario());
        var handler = new JsonWebTokenHandler();

        TokenValidationParameters Parametros(string chave) => new()
        {
            ValidIssuer = Settings.Issuer,
            ValidAudience = Settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave))
        };

        var comAMesmaChave = await handler.ValidateTokenAsync(token, Parametros(Settings.SigningKey));
        var comOutraChave = await handler.ValidateTokenAsync(token, Parametros("outra-chave-completamente-diferente-da-original-123"));

        Assert.True(comAMesmaChave.IsValid);
        Assert.False(comOutraChave.IsValid);
    }
}

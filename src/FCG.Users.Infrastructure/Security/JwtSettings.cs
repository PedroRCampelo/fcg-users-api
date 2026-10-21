using System.Text;
using Microsoft.Extensions.Configuration;

namespace FCG.Users.Infrastructure.Security;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    private const int TamanhoMinimoDaChaveEmBytes = 32;

    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;

    // A chave de assinatura não tem valor padrão em arquivo nenhum: vem da variável de
    // ambiente JWT__SIGNINGKEY. Sem ela (ou com uma chave curta demais) a aplicação nem sobe.
    public static JwtSettings Carregar(IConfiguration configuration)
    {
        var settings = configuration.GetSection(SectionName).Get<JwtSettings>() ?? new JwtSettings();

        if (Encoding.UTF8.GetByteCount(settings.SigningKey) < TamanhoMinimoDaChaveEmBytes)
            throw new InvalidOperationException(
                $"Chave de assinatura do JWT ausente ou curta demais. Defina a variável de ambiente JWT__SIGNINGKEY com pelo menos {TamanhoMinimoDaChaveEmBytes} caracteres.");

        if (string.IsNullOrWhiteSpace(settings.Issuer) || string.IsNullOrWhiteSpace(settings.Audience))
            throw new InvalidOperationException("Configuração 'Jwt:Issuer' e 'Jwt:Audience' são obrigatórias.");

        return settings;
    }
}

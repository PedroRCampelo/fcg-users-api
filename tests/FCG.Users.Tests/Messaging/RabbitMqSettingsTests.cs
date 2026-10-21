using FCG.Users.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace FCG.Users.Tests.Messaging;

public class RabbitMqSettingsTests
{
    private static IConfiguration Configuracao(Dictionary<string, string?> valores) =>
        new ConfigurationBuilder().AddInMemoryCollection(valores).Build();

    private static Dictionary<string, string?> Completa() => new()
    {
        ["RabbitMq:Host"] = "rabbitmq",
        ["RabbitMq:Username"] = "fcg",
        ["RabbitMq:Password"] = "segredo",
        ["RabbitMq:Queues:NotificationsUserCreated"] = "notifications.user-created"
    };

    [Fact]
    public void Carregar_Deve_Ler_Host_Credenciais_E_Nome_Da_Fila_Do_Notifications()
    {
        var settings = RabbitMqSettings.Carregar(Configuracao(Completa()));

        Assert.Equal("rabbitmq", settings.Host);
        Assert.Equal(5672, settings.Port);
        Assert.Equal("/", settings.VirtualHost);
        Assert.Equal("notifications.user-created", settings.Queues.NotificationsUserCreated);
    }

    [Theory]
    [InlineData("RabbitMq:Username", "RabbitMq__Username")]
    [InlineData("RabbitMq:Password", "RabbitMq__Password")]
    [InlineData("RabbitMq:Queues:NotificationsUserCreated", "RabbitMq__Queues__NotificationsUserCreated")]
    public void Carregar_Sem_Um_Valor_Obrigatorio_Deve_Falhar_Apontando_A_Variavel_De_Ambiente(string chave, string variavel)
    {
        var valores = Completa();
        valores.Remove(chave);

        var erro = Assert.Throws<InvalidOperationException>(() => RabbitMqSettings.Carregar(Configuracao(valores)));

        Assert.Contains(variavel, erro.Message);
    }
}

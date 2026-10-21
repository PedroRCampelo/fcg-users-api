using Microsoft.Extensions.Configuration;

namespace FCG.Users.Infrastructure.Messaging;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public QueuesSettings Queues { get; set; } = new();

    public static RabbitMqSettings Carregar(IConfiguration configuration)
    {
        var settings = configuration.GetSection(SectionName).Get<RabbitMqSettings>() ?? new RabbitMqSettings();

        if (string.IsNullOrWhiteSpace(settings.Username) || string.IsNullOrWhiteSpace(settings.Password))
            throw new InvalidOperationException("Credenciais do RabbitMQ ausentes. Defina RabbitMq__Username e RabbitMq__Password.");

        if (string.IsNullOrWhiteSpace(settings.Queues.NotificationsUserCreated))
            throw new InvalidOperationException("Nome da fila do Notifications ausente. Defina RabbitMq__Queues__NotificationsUserCreated.");

        return settings;
    }
}

public class QueuesSettings
{
    // Fila do NotificationsAPI para o UserCreatedEvent. Quem publica o evento (este serviço) a declara
    // no startup, então precisa ler o mesmo nome que o consumidor (no Kubernetes, do mesmo ConfigMap).
    public string NotificationsUserCreated { get; set; } = string.Empty;
}

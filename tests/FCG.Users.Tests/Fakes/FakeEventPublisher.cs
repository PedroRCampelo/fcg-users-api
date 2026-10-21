using FCG.Users.Application.Interfaces;

namespace FCG.Users.Tests.Fakes;

public class FakeEventPublisher : IEventPublisher
{
    private readonly List<string> _chamadas;

    public FakeEventPublisher(List<string> chamadas)
    {
        _chamadas = chamadas;
    }

    public List<object> Publicados { get; } = new();

    public Task PublicarAsync<TEvento>(TEvento evento, CancellationToken cancellationToken = default)
        where TEvento : class
    {
        Publicados.Add(evento);
        _chamadas.Add("publicar");
        return Task.CompletedTask;
    }
}

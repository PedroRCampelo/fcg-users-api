using FCG.Users.Application.Interfaces;

namespace FCG.Users.Tests.Fakes;

public class FakeUnitOfWork : IUnitOfWork
{
    private readonly List<string> _chamadas;

    public FakeUnitOfWork(List<string> chamadas)
    {
        _chamadas = chamadas;
    }

    public int Salvamentos { get; private set; }

    public Task SalvarAsync(CancellationToken cancellationToken = default)
    {
        Salvamentos++;
        _chamadas.Add("salvar");
        return Task.CompletedTask;
    }
}

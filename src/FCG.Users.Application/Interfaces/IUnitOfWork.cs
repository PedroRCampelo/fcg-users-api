namespace FCG.Users.Application.Interfaces;

public interface IUnitOfWork
{
    Task SalvarAsync(CancellationToken cancellationToken = default);
}

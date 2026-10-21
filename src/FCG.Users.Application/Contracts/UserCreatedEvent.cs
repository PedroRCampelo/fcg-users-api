namespace FCG.Contracts;

public sealed record UserCreatedEvent(Guid UserId, string Nome, string Email);

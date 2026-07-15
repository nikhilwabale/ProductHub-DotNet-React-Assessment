namespace Domain.Events;

public sealed record ProductCreatedEvent(int ProductId, DateTime OccurredOn);

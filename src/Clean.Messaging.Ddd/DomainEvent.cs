namespace Clean.Messaging.Ddd;

public interface IDomainEvent
{
    Guid EventId { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
}

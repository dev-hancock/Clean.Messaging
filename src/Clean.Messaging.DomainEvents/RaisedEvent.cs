namespace Clean.Messaging.DomainEvents;

public sealed record RaisedEvent(
    Guid Id,
    object Message);

public interface IDomainEventSource
{
    IReadOnlyCollection<RaisedEvent> Events { get; }

    void Acknowledge(
        Guid eventId);
}

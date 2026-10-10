namespace Clean.Messaging.DomainEvents;

public abstract class AggregateRoot
    : IDomainEventSource
{
    private readonly List<RaisedEvent> _events = [];

    protected void Raise<TEvent>(
        TEvent message)
        where TEvent : notnull
    {
        ArgumentNullException.ThrowIfNull(
            message);

        _events.Add(
            new(
                Guid.NewGuid(),
                message));
    }

    IReadOnlyCollection<RaisedEvent> IDomainEventSource.Events =>
        _events;

    void IDomainEventSource.Acknowledge(
        Guid eventId)
    {
        _events.RemoveAll(
            @event =>
                @event.Id == eventId);
    }
}

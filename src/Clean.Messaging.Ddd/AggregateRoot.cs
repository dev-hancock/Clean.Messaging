namespace Clean.Messaging.Ddd;

public abstract class AggregateRoot
{
    private readonly List<DomainEvent> _events = [];

    public IReadOnlyCollection<DomainEvent> Events => _events.AsReadOnly();

    protected void Raise(DomainEvent message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _events.Add(message);
    }

    public void ClearEvents()
    {
        _events.Clear();
    }

    public void Acknowledge(Guid eventId)
    {
        _events.RemoveAll(message => message.EventId == eventId);
    }
}

namespace Clean.Messaging.Outbox.Transport;

public interface IOutboxTransport
{
    string Name { get; }

    ValueTask Dispatch(
        OutboxEnvelope envelope,
        CancellationToken cancellationToken);
}

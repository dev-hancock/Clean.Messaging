namespace Clean.Messaging.Outbox;

public sealed record OutboxDispatch(
    Guid MessageId,
    Guid EventId,
    string TargetId,
    string? Destination,
    string Contract,
    ReadOnlyMemory<byte> Payload,
    Guid? CorrelationId,
    Guid? CausationId);


public interface IOutboxTransport
{
    string Name { get; }

    ValueTask Dispatch(
        OutboxDispatch message,
        CancellationToken cancellationToken);
}
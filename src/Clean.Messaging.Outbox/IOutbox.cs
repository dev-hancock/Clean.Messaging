namespace Clean.Messaging.Outbox;

public sealed record OutboxMessage<T>(
    Guid Id,
    T Message,
    Guid? CorrelationId = null,
    Guid? CausationId = null);

public interface IOutbox
{
    void Enqueue<TMessage>(
        OutboxMessage<TMessage> message)
        where TMessage : notnull;
}

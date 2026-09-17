namespace Clean.Messaging.Inbox;

public sealed record InboxMessage<T>(
    Guid Id,
    T Message,
    Guid? CorrelationId = null,
    Guid? CausationId = null,
    Guid? EventId = null);

public interface IInbox
{
    ValueTask Accept<T>(
        InboxMessage<T> message,
        CancellationToken cancellationToken = default)
        where T : notnull;
}
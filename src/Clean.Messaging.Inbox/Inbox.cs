using Clean.Messaging.Consumers;
using Clean.Messaging.Serialization;

namespace Clean.Messaging.Inbox;

internal sealed class Inbox(
    IInboxStore store,
    IConsumerRegistry consumers,
    IMessageSerializer serializer,
    InboxSignal signal,
    TimeProvider time)
    : IInbox
{
    public async ValueTask Accept<TMessage>(
        InboxMessage<TMessage> message,
        CancellationToken cancellationToken = default)
        where TMessage : notnull
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Message);

        if (message.Id == Guid.Empty)
        {
            throw new ArgumentException(
                "Message ID cannot be empty.",
                nameof(message));
        }

        var eventId =
            message.EventId ??
            message.Id;

        if (eventId == Guid.Empty)
        {
            throw new ArgumentException(
                "Event ID cannot be empty.",
                nameof(message));
        }

        var data =
            serializer.Serialize(
                message.Message);

        var createdAt =
            time.GetUtcNow()
                .UtcDateTime;

        var entries = consumers
            .Get(message.Message.GetType())
            .Select(consumer =>
                new InboxEntry
                {
                    MessageId = message.Id,
                    EventId = eventId,
                    TargetId = consumer.ConsumerId,
                    Message = data,
                    CorrelationId = message.CorrelationId,
                    CausationId = message.CausationId,
                    CreatedAt = createdAt
                })
            .ToArray();

        var batch = new InboxBatch(
            message.Id,
            eventId,
            data,
            message.CorrelationId,
            message.CausationId,
            createdAt,
            entries);

        await store.Accept(
            batch,
            cancellationToken);

        if (!batch.IsEmpty)
        {
            signal.Wake();
        }
    }
}
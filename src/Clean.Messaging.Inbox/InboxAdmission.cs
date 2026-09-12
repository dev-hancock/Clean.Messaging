using Clean.Messaging.Serialization;

namespace Clean.Messaging.Inbox;

internal sealed class InboxAdmission
{
    public Guid MessageId { get; init; }

    public Guid EventId { get; init; }

    public required MessageData Message { get; init; }

    public Guid? CorrelationId { get; init; }

    public Guid? CausationId { get; init; }

    public DateTime CreatedAt { get; init; }
}
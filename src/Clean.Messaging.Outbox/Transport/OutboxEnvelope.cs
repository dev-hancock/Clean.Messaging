using Clean.Messaging.Serialization;

namespace Clean.Messaging.Outbox.Transport;

public sealed record OutboxEnvelope(
    Guid MessageId,
    Guid EventId,
    string TargetId,
    string? Destination,
    MessageData Message,
    Guid? CorrelationId,
    Guid? CausationId);

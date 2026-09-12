namespace Clean.Messaging.Abstractions;

public interface IMessageContext
{
    Guid? MessageId { get; }

    Guid? CorrelationId { get; }

    Guid? CausationId { get; }

    string? ConsumerId { get; }
}
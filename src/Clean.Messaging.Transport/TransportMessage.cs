namespace Clean.Messaging.Transport;

public sealed record TransportMessage
{
    public Guid? MessageId { get; init; }

    public Guid? EventId { get; init; }

    public required string Contract { get; init; }

    public required ReadOnlyMemory<byte> Payload { get; init; }

    public Guid? CorrelationId { get; init; }

    public Guid? CausationId { get; init; }

    public string? ContentType { get; init; }

    public IReadOnlyList<TransportHeader> Headers { get; init; } = [];
}
using Clean.Messaging.Abstractions;
using Clean.Messaging.Failures;
using Clean.Messaging.Persistence;
using Clean.Messaging.Serialization;

namespace Clean.Messaging.Inbox;

internal sealed class InboxEntry : IDurableEntry
{
    public Guid MessageId { get; init; }

    public Guid EventId { get; init; }

    public required string ConsumerId { get; init; }

    public required MessageData Message { get; init; }

    public Guid? CorrelationId { get; init; }

    public Guid? CausationId { get; init; }

    public DateTime CreatedAt { get; init; }

    public int Attempts { get; set; }

    public string? ClaimId { get; set; }

    public DateTime? ClaimedUntil { get; set; }

    public DateTime? NextAttemptAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? DeadLetteredAt { get; set; }

    public DateTime? DiscardedAt { get; set; }

    public FailureCategory? FailureCategory { get; set; }

    public string? FailureCode { get; set; }

    public string? ExceptionType { get; set; }

    public string? FailureMessage { get; set; }

    public DateTime? LastFailedAt { get; set; }

    public int? LastFailedAttempt { get; set; }

    public EntryKey Key => new(MessageId, ConsumerId);
}

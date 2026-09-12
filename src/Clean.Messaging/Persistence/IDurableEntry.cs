using Clean.Messaging.Abstractions;
using Clean.Messaging.Failures;
using Clean.Messaging.Serialization;

namespace Clean.Messaging.Persistence;

public interface IDurableEntry
{
    Guid MessageId { get; }

    Guid EventId { get; }

    string ConsumerId { get; }

    MessageData Message { get; }

    Guid? CorrelationId { get; }

    Guid? CausationId { get; }

    DateTime CreatedAt { get; }

    int Attempts { get; set; }

    string? ClaimId { get; set; }

    DateTime? ClaimedUntil { get; set; }

    DateTime? NextAttemptAt { get; set; }

    DateTime? ProcessedAt { get; set; }

    DateTime? DeadLetteredAt { get; set; }

    DateTime? DiscardedAt { get; set; }

    FailureCategory? FailureCategory { get; set; }

    string? FailureCode { get; set; }

    string? ExceptionType { get; set; }

    string? FailureMessage { get; set; }

    DateTime? LastFailedAt { get; set; }

    int? LastFailedAttempt { get; set; }

    EntryKey Key { get; }
}

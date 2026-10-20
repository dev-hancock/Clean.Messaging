using Clean.Messaging.Abstractions;
using Clean.Messaging.Failures;
using Clean.Messaging.Inbox.Persistence;
using Clean.Messaging.Serialization;

namespace Clean.Messaging.EntityFrameworkCore.Tests.Infrastructure;

internal sealed record EntryState(
    EntryKey Key,
    Guid EventId,
    MessageData Message,
    Guid? CorrelationId,
    Guid? CausationId,
    DateTime CreatedAt,
    int Attempts,
    string? ClaimId,
    DateTime? ClaimedUntil,
    DateTime? NextAttemptAt,
    DateTime? ProcessedAt,
    DateTime? DeadLetteredAt,
    DateTime? DiscardedAt,
    FailureCategory? FailureCategory,
    string? FailureCode,
    string? ExceptionType,
    string? FailureMessage,
    DateTime? LastFailedAt,
    int? LastFailedAttempt)
{
    public static EntryState From(InboxEntry entry) => new(
        entry.Key, entry.EventId, entry.Message, entry.CorrelationId, entry.CausationId,
        entry.CreatedAt, entry.Attempts, entry.ClaimId, entry.ClaimedUntil, entry.NextAttemptAt,
        entry.ProcessedAt, entry.DeadLetteredAt, entry.DiscardedAt, entry.FailureCategory,
        entry.FailureCode, entry.ExceptionType, entry.FailureMessage, entry.LastFailedAt,
        entry.LastFailedAttempt);
}

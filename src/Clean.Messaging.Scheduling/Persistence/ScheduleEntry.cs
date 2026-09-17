using Clean.Messaging.Serialization;

namespace Clean.Messaging.Scheduling.Persistence;

internal sealed class ScheduleEntry
{
    public ScheduleId Id { get; init; }

    public ScheduleGroupId? GroupId { get; init; }

    public required ScheduleTarget Target { get; init; }

    public required MessageData Message { get; init; }

    public DateTime DueAt { get; init; }

    public Guid? CorrelationId { get; init; }

    public Guid? CausationId { get; init; }

    public long Version { get; private set; }

    public DateTime CreatedAt { get; init; }

    public int Attempts { get; set; }

    public string? ClaimId { get; set; }

    public DateTime? ClaimedUntil { get; set; }

    public DateTime? NextAttemptAt { get; set; }

    public DateTime? DispatchedAt { get; private set; }

    public DateTime? CancelledAt { get; private set; }

    public DateTime? FailedAt { get; set; }

    public string? FailureCode { get; set; }

    public string? FailureMessage { get; set; }

    public string? ExceptionType { get; set; }

    public bool IsTerminal =>
        DispatchedAt is not null ||
        CancelledAt is not null ||
        FailedAt is not null;

    public void Complete(
        DateTime now)
    {
        ClaimId = null;
        ClaimedUntil = null;
        NextAttemptAt = null;
        DispatchedAt = now;
    }

    public void Cancel(
        DateTime now)
    {
        if (IsTerminal)
        {
            return;
        }

        Version++;

        ClaimId = null;
        ClaimedUntil = null;
        NextAttemptAt = null;
        CancelledAt = now;
    }
}
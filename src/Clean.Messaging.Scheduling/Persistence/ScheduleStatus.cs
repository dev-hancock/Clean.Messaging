using Clean.Messaging.Serialization;

namespace Clean.Messaging.Scheduling.Persistence;

public sealed record ScheduleStatus(
    ScheduleId ScheduleId,
    ScheduleGroupId? ScheduleGroupId,
    ScheduleTarget Target,
    MessageData MessageData,
    DateTime DueAt,
    int Attempts,
    DateTime? NextAttemptAt,
    DateTime? DispatchedAt,
    DateTime? CancelledAt,
    DateTime? FailedAt,
    string? FailureCode,
    string? FailureMessage);
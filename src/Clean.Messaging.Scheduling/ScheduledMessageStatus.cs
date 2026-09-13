using Clean.Messaging.Serialization;

namespace Clean.Messaging.Scheduling;

public sealed record ScheduledMessageStatus(
    ScheduleId ScheduleId,
    ScheduleGroupId? ScheduleGroupId,
    ScheduledMessageTarget Target,
    MessageData MessageData,
    DateTime DueAt,
    int Attempts,
    DateTime? NextAttemptAt,
    DateTime? DispatchedAt,
    DateTime? CancelledAt,
    DateTime? FailedAt,
    string? FailureCode,
    string? FailureMessage);

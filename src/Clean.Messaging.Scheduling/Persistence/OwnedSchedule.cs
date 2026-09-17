namespace Clean.Messaging.Scheduling.Persistence;

internal sealed record OwnedSchedule(
    ScheduleEntry Message,
    string ClaimId);
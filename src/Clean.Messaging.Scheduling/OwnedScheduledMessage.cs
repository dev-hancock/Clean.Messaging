namespace Clean.Messaging.Scheduling;

internal sealed record OwnedScheduledMessage(
    ScheduledMessageEntry Message,
    string ClaimId);

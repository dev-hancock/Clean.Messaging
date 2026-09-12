namespace Clean.Messaging.Abstractions;

public sealed record DeadLetter(
    EntryKey Key,
    Guid EventId,
    DateTime DeadLetteredAt,
    int Attempts,
    string? Code,
    string? Error);

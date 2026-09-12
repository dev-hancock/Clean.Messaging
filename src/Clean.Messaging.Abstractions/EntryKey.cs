namespace Clean.Messaging.Abstractions;

public readonly record struct EntryKey(
    Guid MessageId,
    string TargetId);
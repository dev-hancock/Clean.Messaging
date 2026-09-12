namespace Clean.Messaging.Inbox;

public readonly record struct ReplayResult(
    int Matched,
    int Replayed);
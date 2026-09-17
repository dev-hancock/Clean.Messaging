namespace Clean.Messaging.Inbox.Runtime;

public readonly record struct ReplayResult(
    int Matched,
    int Replayed);
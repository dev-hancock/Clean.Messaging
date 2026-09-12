namespace Clean.Messaging.Abstractions;

public readonly record struct RequeueResult(
    int Requested,
    int Requeued);
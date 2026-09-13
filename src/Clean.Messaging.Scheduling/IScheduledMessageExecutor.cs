namespace Clean.Messaging.Scheduling;

internal interface IScheduledMessageExecutor
{
    ValueTask<bool> Dispatch(
        OwnedScheduledMessage owned,
        CancellationToken cancellationToken);
}

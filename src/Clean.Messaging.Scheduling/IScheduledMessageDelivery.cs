namespace Clean.Messaging.Scheduling;

internal interface IScheduledMessageDelivery
{
    ScheduledMessageTarget Target { get; }

    ValueTask Dispatch(
        ScheduledMessageEntry message,
        CancellationToken cancellationToken);
}

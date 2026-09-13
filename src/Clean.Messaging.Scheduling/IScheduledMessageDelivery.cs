namespace Clean.Messaging.Scheduling;

public interface IScheduledMessageDelivery
{
    ScheduledMessageTarget Target { get; }

    ValueTask Dispatch(
        ScheduledMessageDispatch message,
        CancellationToken cancellationToken);
}

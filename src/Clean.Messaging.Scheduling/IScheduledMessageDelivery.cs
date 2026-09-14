namespace Clean.Messaging.Scheduling;

/// <summary>
/// Dispatches scheduled messages under at-least-once delivery semantics.
/// Implementations must tolerate duplicate delivery for the same scheduled message.
/// </summary>
public interface IScheduledMessageDelivery
{
    ScheduledMessageTarget Target { get; }

    /// <summary>
    /// Dispatches a scheduled message to its destination.
    /// The caller may invoke this more than once for the same scheduled message when retries or recovery occur.
    /// </summary>
    ValueTask Dispatch(
        ScheduledMessageDispatch message,
        CancellationToken cancellationToken);
}

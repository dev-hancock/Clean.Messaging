namespace Clean.Messaging.Scheduling.Delivery;

/// <summary>
///     Dispatches scheduled messages under at-least-once delivery semantics.
///     Implementations must tolerate duplicate delivery for the same scheduled message.
/// </summary>
public interface IScheduleDelivery
{
    ScheduleTarget Target { get; }

    /// <summary>
    ///     Dispatches a scheduled message to its destination.
    ///     The caller may invoke this more than once for the same scheduled message when retries or recovery occur.
    /// </summary>
    ValueTask Dispatch(
        ScheduledDispatch message,
        CancellationToken cancellationToken);
}
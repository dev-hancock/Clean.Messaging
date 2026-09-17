namespace Clean.Messaging.Scheduling;

/// <summary>
///     Schedules messages for deferred delivery.
///     Retries of the same logical scheduling operation must reuse the same <see cref="ScheduleId" />;
///     create a new <see cref="ScheduleId" /> only for a distinct scheduled message.
/// </summary>
public interface IMessageScheduler
{
    /// <summary>
    ///     Schedules a message for delivery at the specified time.
    ///     Retries of the same logical scheduling operation must reuse <paramref name="id" />;
    ///     use a different <paramref name="id" /> only for a distinct scheduled message.
    /// </summary>
    ScheduleId Schedule<TMessage>(
        ScheduleId id,
        TMessage message,
        DateTimeOffset dueAt,
        ScheduleGroupId? groupId = null,
        Guid? correlationId = null,
        Guid? causationId = null)
        where TMessage : notnull;

    ValueTask Cancel(
        ScheduleId id,
        CancellationToken cancellationToken = default);

    ValueTask CancelGroup(
        ScheduleGroupId groupId,
        CancellationToken cancellationToken = default);
}
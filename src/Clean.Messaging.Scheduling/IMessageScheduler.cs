namespace Clean.Messaging.Scheduling;

public interface IMessageScheduler
{
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

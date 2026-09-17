namespace Clean.Messaging.Scheduling.Persistence;

public interface IScheduleWriter
{
    ScheduleId Schedule<TMessage>(
        ScheduleId id,
        TMessage message,
        DateTimeOffset dueAt,
        ScheduleTarget target,
        ScheduleGroupId? groupId,
        Guid? correlationId,
        Guid? causationId)
        where TMessage : notnull;

    ValueTask Cancel(
        ScheduleId id,
        ScheduleTarget target,
        ScheduleGroupId? groupId,
        CancellationToken cancellationToken);

    ValueTask CancelGroup(
        ScheduleGroupId groupId,
        ScheduleTarget target,
        ScheduleId? exceptId,
        CancellationToken cancellationToken);
}
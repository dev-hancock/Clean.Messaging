namespace Clean.Messaging.Scheduling;

public interface IScheduledMessageWriter
{
    ScheduleId Schedule<TMessage>(
        ScheduleId id,
        TMessage message,
        DateTimeOffset dueAt,
        ScheduledMessageTarget target,
        ScheduleGroupId? groupId,
        Guid? correlationId,
        Guid? causationId)
        where TMessage : notnull;

    ValueTask Cancel(
        ScheduleId id,
        ScheduledMessageTarget target,
        ScheduleGroupId? groupId,
        CancellationToken cancellationToken);

    ValueTask CancelGroup(
        ScheduleGroupId groupId,
        ScheduledMessageTarget target,
        ScheduleId? exceptId,
        CancellationToken cancellationToken);
}

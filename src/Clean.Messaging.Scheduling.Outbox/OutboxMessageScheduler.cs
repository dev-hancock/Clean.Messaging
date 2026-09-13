namespace Clean.Messaging.Scheduling.Outbox;

internal sealed class OutboxMessageScheduler(
    IScheduledMessageWriter writer)
    : IMessageScheduler
{
    public ScheduleId Schedule<TMessage>(
         ScheduleId id,
         TMessage message,
         DateTimeOffset dueAt,
         ScheduleGroupId? groupId = null,
         Guid? correlationId = null,
         Guid? causationId = null)
         where TMessage : notnull
    {
        return writer.Schedule(
            id,
            message,
            dueAt,
            ScheduledMessageTarget.Message,
            groupId,
            correlationId,
            causationId);
    }

    public ValueTask Cancel(
        ScheduleId id,
        CancellationToken cancellationToken = default)
    {
        return writer.Cancel(
            id,
            ScheduledMessageTarget.Message,
            null,
            cancellationToken);
    }

    public ValueTask CancelGroup(
        ScheduleGroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return writer.CancelGroup(
            groupId,
            ScheduledMessageTarget.Message,
            null,
            cancellationToken);
    }
}

namespace Clean.Messaging.Scheduling;

public interface IScheduledMessageReader
{
    ValueTask<IReadOnlyList<ScheduledMessageStatus>> GetGroup(
        ScheduleGroupId groupId,
        ScheduledMessageTarget? target,
        CancellationToken cancellationToken);
}

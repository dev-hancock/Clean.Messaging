namespace Clean.Messaging.Scheduling.Persistence;

public interface IScheduleReader
{
    ValueTask<IReadOnlyList<ScheduleStatus>> GetGroup(
        ScheduleGroupId groupId,
        ScheduleTarget? target,
        CancellationToken cancellationToken);
}
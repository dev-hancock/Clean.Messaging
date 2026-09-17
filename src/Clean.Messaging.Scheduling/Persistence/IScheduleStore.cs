namespace Clean.Messaging.Scheduling.Persistence;

internal interface IScheduleStore
{
    void Add(
        ScheduleEntry message);

    ValueTask Cancel(
        ScheduleId id,
        ScheduleGroupId? groupId,
        ScheduleTarget? target,
        DateTime now,
        CancellationToken cancellationToken);

    ValueTask CancelGroup(
        ScheduleGroupId groupId,
        ScheduleTarget? target,
        ScheduleId? exceptId,
        DateTime now,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<ScheduleEntry>> GetGroup(
        ScheduleGroupId groupId,
        ScheduleTarget? target,
        CancellationToken cancellationToken);

    ValueTask<DateTime?> NextDue(
        DateTime now,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<OwnedSchedule>> Claim(
        DateTime now,
        int limit,
        TimeSpan lease,
        CancellationToken cancellationToken);

    ValueTask<bool> Retry(
        OwnedSchedule owned,
        ScheduleFailure failure,
        DateTime now,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken);

    ValueTask<bool> Fail(
        OwnedSchedule owned,
        ScheduleFailure failure,
        DateTime now,
        CancellationToken cancellationToken);

    ValueTask<int> Cleanup(
        DateTime before,
        int limit,
        CancellationToken cancellationToken);
}
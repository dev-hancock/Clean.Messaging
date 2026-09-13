namespace Clean.Messaging.Scheduling;

internal interface IScheduledMessageStore
{
    void Add(
        ScheduledMessageEntry message);

    ValueTask Cancel(
        ScheduleId id,
        ScheduleGroupId? groupId,
        ScheduledMessageTarget? target,
        DateTime now,
        CancellationToken cancellationToken);

    ValueTask CancelGroup(
        ScheduleGroupId groupId,
        ScheduledMessageTarget? target,
        ScheduleId? exceptId,
        DateTime now,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<ScheduledMessageEntry>> GetGroup(
        ScheduleGroupId groupId,
        ScheduledMessageTarget? target,
        CancellationToken cancellationToken);

    ValueTask<DateTime?> NextDue(
        DateTime now,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<OwnedScheduledMessage>> Claim(
        DateTime now,
        int limit,
        TimeSpan lease,
        CancellationToken cancellationToken);

    ValueTask<bool> Retry(
        OwnedScheduledMessage owned,
        ScheduledMessageFailure failure,
        DateTime now,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken);

    ValueTask<bool> Fail(
        OwnedScheduledMessage owned,
        ScheduledMessageFailure failure,
        DateTime now,
        CancellationToken cancellationToken);

    ValueTask<int> Cleanup(
        DateTime before,
        int limit,
        CancellationToken cancellationToken);
}

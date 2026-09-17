using Clean.Messaging.Scheduling.Persistence;

namespace Clean.Messaging.Scheduling.Runtime;

internal interface IScheduleExecutor
{
    ValueTask<bool> Dispatch(
        OwnedSchedule owned,
        CancellationToken cancellationToken);
}
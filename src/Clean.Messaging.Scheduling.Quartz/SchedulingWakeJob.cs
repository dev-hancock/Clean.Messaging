using Clean.Messaging.Scheduling.Runtime;
using Quartz;

namespace Clean.Messaging.Scheduling.Quartz;

internal sealed class SchedulingWakeJob(
    ISchedulingEngine engine)
    : IJob
{
    public ValueTask Execute(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        engine.Wake();

        return ValueTask.CompletedTask;
    }
}

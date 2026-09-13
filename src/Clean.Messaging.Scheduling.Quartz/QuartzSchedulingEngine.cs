using Microsoft.Extensions.Logging;
using Quartz;

namespace Clean.Messaging.Scheduling.Quartz;

internal sealed partial class QuartzSchedulingEngine(
    IScheduler scheduler,
    TimeProvider time,
    ILogger<QuartzSchedulingEngine> logger)
    : ISchedulingEngine
{
    private static readonly JobKey Job = new(
        "scheduler-wake",
        "Clean.Messaging");

    private static readonly TriggerKey Trigger = new(
        "scheduler-wake",
        "Clean.Messaging");

    private readonly SchedulingEngine _fallback =
        new(time);

    private readonly SemaphoreSlim _schedule =
        new(1, 1);

    public void Wake()
    {
        _fallback.Wake();
    }

    public async ValueTask Wait(
        DateTime wakeAt,
        CancellationToken cancellationToken)
    {
        var dueAt = new DateTimeOffset(
            DateTime.SpecifyKind(
                wakeAt,
                DateTimeKind.Utc));

        if (dueAt <= time.GetUtcNow())
        {
            return;
        }

        try
        {
            await Schedule(
                dueAt,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            QuartzScheduleFailed(
                logger,
                exception,
                dueAt);
        }

        await _fallback.Wait(
            wakeAt,
            cancellationToken);
    }

    private async ValueTask Schedule(
        DateTimeOffset dueAt,
        CancellationToken cancellationToken)
    {
        await _schedule.WaitAsync(
            cancellationToken);

        try
        {
            var job = JobBuilder
                .Create<SchedulingWakeJob>()
                .WithIdentity(Job)
                .Build();

            var trigger = TriggerBuilder
                .Create()
                .WithIdentity(Trigger)
                .ForJob(Job)
                .StartAt(dueAt)
                .Build();

            await scheduler.ScheduleJob(
                job,
                trigger,
                ScheduleJobOptions.Replacing,
                cancellationToken);
        }
        finally
        {
            _schedule.Release();
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Quartz failed to schedule the messaging wake trigger for '{WakeAt}'. Local recovery remains active.")]
    private static partial void QuartzScheduleFailed(
        ILogger logger,
        Exception exception,
        DateTimeOffset wakeAt);
}
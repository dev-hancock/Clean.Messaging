using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Channels;

namespace Clean.Messaging.Scheduling.Quartz;

internal sealed partial class SchedulingEngine(
    IScheduler scheduler,
    TimeProvider time,
    ILogger<SchedulingEngine> logger)
    : ISchedulingEngine
{
    private static readonly JobKey Job = new(
        "scheduler-wake",
        "Clean.Messaging");

    private static readonly TriggerKey Trigger = new(
        "scheduler-wake",
        "Clean.Messaging");

    private readonly SemaphoreSlim _schedule =
        new(1, 1);

    private readonly Channel<byte> _wake =
        Channel.CreateBounded<byte>(
            new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = false,
                SingleWriter = false
            });

    public void Wake()
    {
        _wake.Writer.TryWrite(0);
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
            ScheduleFailed(
                logger,
                exception,
                dueAt);
        }

        await WaitLocally(
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

    private async ValueTask WaitLocally(
        DateTime wakeAt,
        CancellationToken cancellationToken)
    {
        var delay = wakeAt - time.GetUtcNow().UtcDateTime;

        if (delay <= TimeSpan.Zero)
        {
            return;
        }

        using var wait = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);

        var signalled = _wake.Reader
            .ReadAsync(wait.Token)
            .AsTask();

        var elapsed = Task.Delay(
            delay,
            time,
            wait.Token);

        try
        {
            var completed = await Task.WhenAny(
                signalled,
                elapsed);

            await completed;
        }
        finally
        {
            await wait.CancelAsync();

            try
            {
                await Task.WhenAll(
                    signalled,
                    elapsed);
            }
            catch (OperationCanceledException)
                when (wait.IsCancellationRequested)
            {
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        while (_wake.Reader.TryRead(out _))
        {
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Quartz failed to schedule the messaging wake trigger for '{WakeAt}'. Local recovery remains active.")]
    private static partial void ScheduleFailed(
        ILogger logger,
        Exception exception,
        DateTimeOffset wakeAt);
}
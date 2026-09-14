using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Scheduling;

internal sealed partial class SchedulingCleanupWorker(
    IServiceScopeFactory scopes,
    TimeProvider time,
    IOptions<SchedulingOptions> options,
    ILogger<SchedulingCleanupWorker> logger)
    : BackgroundService
{
    private readonly SchedulingOptions _options = options.Value;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            _options.Cleanup.Interval,
            time);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await Cleanup(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                Failed(
                    logger,
                    exception);
            }
        }
    }

    internal async Task Cleanup(
        CancellationToken cancellationToken)
    {
        var cleanup = _options.Cleanup;

        await using var scope = scopes.CreateAsyncScope();

        var store = scope.ServiceProvider
            .GetRequiredService<IScheduledMessageStore>();

        var before = time.GetUtcNow()
            .Subtract(cleanup.Retention)
            .UtcDateTime;

        for (var batch = 0;
            batch < cleanup.MaxBatchesPerRun;
            batch++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var deleted = await store.Cleanup(
                before,
                cleanup.BatchSize,
                cancellationToken);

            if (deleted < cleanup.BatchSize)
            {
                return;
            }
        }

        BatchCapReached(
            logger,
            cleanup.MaxBatchesPerRun,
            cleanup.BatchSize);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Scheduled message cleanup failed.")]
    private static partial void Failed(
        ILogger logger,
        Exception exception);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Scheduled message cleanup reached the per-run cap of {MaxBatchesPerRun} batches at {BatchSize} items per batch; additional cleanup will wait for the next interval.")]
    private static partial void BatchCapReached(
        ILogger logger,
        int maxBatchesPerRun,
        int batchSize);
}

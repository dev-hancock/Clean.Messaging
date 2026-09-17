using Clean.Messaging.Outbox.Configuration;
using Clean.Messaging.Outbox.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Outbox;

public sealed partial class OutboxCleanupWorker(
    IOutboxStore store,
    TimeProvider time,
    IOptions<OutboxOptions> options,
    ILogger<OutboxCleanupWorker> logger) : BackgroundService
{
    private readonly MessageCleanupOptions _options = options.Value.Cleanup;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cleanup = options.Value.Cleanup;
        using var timer = new PeriodicTimer(_options.Interval, time);

        try
        {
            do
            {
                await Cleanup(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task Cleanup(
        CancellationToken cancellationToken)
    {
        try
        {
            var before = time
                .GetUtcNow()
                .Subtract(_options.Retention)
                .UtcDateTime;

            await store.Cleanup(
                before,
                _options.BatchSize,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            CleanupFailed(
                logger,
                exception);
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Outbox cleanup failed.")]
    private static partial void CleanupFailed(
        ILogger logger,
        Exception exception);
}

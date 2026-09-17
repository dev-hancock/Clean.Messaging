using Clean.Messaging.Inbox.Configuration;
using Clean.Messaging.Inbox.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Inbox.Runtime;

public sealed partial class InboxCleanupWorker(
    IInboxStore store,
    TimeProvider time,
    IOptions<InboxOptions> options,
    ILogger<InboxCleanupWorker> logger) : BackgroundService
{
    private readonly MessageCleanupOptions _options = options.Value.Cleanup;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.Interval, time);

        try
        {
            do
            {
                await Cleanup(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
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
        Message = "Inbox cleanup failed.")]
    private static partial void CleanupFailed(
        ILogger logger,
        Exception exception);
}
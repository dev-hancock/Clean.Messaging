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
                CleanupFailed(
                    logger,
                    exception);
            }
        }
    }

    private async Task Cleanup(
        CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();

        var store = scope.ServiceProvider
            .GetRequiredService<IScheduledMessageStore>();

        var before = time.GetUtcNow()
            .Subtract(_options.Cleanup.Retention)
            .UtcDateTime;

        await store.Cleanup(
            before,
            _options.Cleanup.BatchSize,
            cancellationToken);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Scheduled message cleanup failed.")]
    private static partial void CleanupFailed(
        ILogger logger,
        Exception exception);
}

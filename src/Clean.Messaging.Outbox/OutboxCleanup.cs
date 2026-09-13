using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Outbox;

public sealed class OutboxCleanup(
    IServiceScopeFactory scopes,
    TimeProvider time,
    IOptions<OutboxOptions> options,
    ILogger<OutboxCleanup> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cleanup = options.Value.Cleanup;
        using var timer = new PeriodicTimer(cleanup.Interval, time);

        try
        {
            do
            {
                await Cleanup(cleanup, stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task Cleanup(
        MessageCleanupOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            var before = time.GetUtcNow().Subtract(options.Retention).UtcDateTime;

            await store.Cleanup(
                before,
                options.BatchSize,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Outbox cleanup failed.");
        }
    }
}

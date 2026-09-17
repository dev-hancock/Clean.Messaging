using Clean.Messaging.Sagas.Configuration;
using Clean.Messaging.Sagas.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Sagas;

internal sealed partial class SagaCleanupWorker(
    IServiceScopeFactory scopes,
    TimeProvider time,
    IOptions<SagaOptions> options,
    ILogger<SagaCleanupWorker> logger)
    : BackgroundService
{
    private readonly SagaOptions _options = options.Value;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            _options.CleanupInterval,
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

        var sagas = scope.ServiceProvider
            .GetRequiredService<ISagaStore>();

        var now = time.GetUtcNow().UtcDateTime;
        var before = now - _options.Retention;

        await sagas.PurgeState(
            before,
            now,
            _options.CleanupBatchSize,
            cancellationToken);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Saga cleanup failed.")]
    private static partial void CleanupFailed(
        ILogger logger,
        Exception exception);
}
using Clean.Messaging.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Scheduling;

internal sealed partial class SchedulingWorker(
    IServiceScopeFactory scopes,
    ISchedulingEngine engine,
    TimeProvider time,
    IOptions<SchedulingOptions> options,
    ILogger<SchedulingWorker> logger)
    : BackgroundService
{
    private readonly SchedulingOptions _options = options.Value;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDue(stoppingToken);

                await engine.Wait(
                    await GetWakeAt(stoppingToken),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                WorkerFailed(
                    logger,
                    exception);

                await Task.Delay(
                    _options.Recovery.Interval,
                    time,
                    stoppingToken);
            }
        }
    }

    private async Task ProcessDue(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<OwnedScheduledMessage> messages;

        await using (var scope = scopes.CreateAsyncScope())
        {
            var store = scope.ServiceProvider
                .GetRequiredService<IScheduledMessageStore>();

            messages = await store.Claim(
                time.GetUtcNow().UtcDateTime,
                _options.Recovery.BatchSize,
                _options.Lease.Duration,
                cancellationToken);
        }

        if (messages.Count == 0)
        {
            return;
        }

        await Parallel.ForEachAsync(
            messages,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = _options.Processing.Concurrency,
                CancellationToken = cancellationToken
            },
            Process);
    }

    private async ValueTask Process(
        OwnedScheduledMessage owned,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();

            var executor = scope.ServiceProvider
                .GetRequiredService<IScheduledMessageExecutor>();

            var completed = await executor.Dispatch(
                owned,
                cancellationToken);

            if (!completed)
            {
                ClaimLost(
                    logger,
                    owned.Message.Id.Value);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            await Settle(
                owned,
                exception,
                cancellationToken);
        }
    }

    private async ValueTask Settle(
        OwnedScheduledMessage owned,
        Exception exception,
        CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();

        var store = scope.ServiceProvider
            .GetRequiredService<IScheduledMessageStore>();

        var now = time.GetUtcNow().UtcDateTime;
        var failure = ScheduledMessageFailure.From(exception);

        if (ShouldFail(
                owned,
                exception))
        {
            var failed = await store.Fail(
                owned,
                failure,
                now,
                cancellationToken);

            if (!failed)
            {
                ClaimLost(
                    logger,
                    owned.Message.Id.Value);

                return;
            }

            MessageFailed(
                logger,
                exception,
                owned.Message.Id.Value,
                owned.Message.Attempts);

            return;
        }

        var delay = RetryDelay(
            owned.Message.Attempts);

        var retried = await store.Retry(
            owned,
            failure,
            now,
            now.Add(delay),
            cancellationToken);

        if (!retried)
        {
            ClaimLost(
                logger,
                owned.Message.Id.Value);

            return;
        }

        MessageRetry(
            logger,
            exception,
            owned.Message.Id.Value,
            owned.Message.Attempts,
            delay);

        engine.Wake();
    }

    private bool ShouldFail(
        OwnedScheduledMessage owned,
        Exception exception)
    {
        if (exception is MessageException { IsTransient: false })
        {
            return true;
        }

        return owned.Message.Attempts >=
               _options.Retry.MaxAttempts;
    }

    private TimeSpan RetryDelay(
        int attempt)
    {
        var multiplier = Math.Pow(
            2,
            Math.Max(0, attempt - 1));

        var milliseconds = Math.Min(
            _options.Retry.MaxDelay.TotalMilliseconds,
            _options.Retry.Delay.TotalMilliseconds * multiplier);

        return TimeSpan.FromMilliseconds(
            milliseconds);
    }

    private async ValueTask<DateTime> GetWakeAt(
        CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();

        var store = scope.ServiceProvider
            .GetRequiredService<IScheduledMessageStore>();

        var now = time.GetUtcNow().UtcDateTime;
        var recovery = now.Add(_options.Recovery.Interval);

        var next = await store.NextDue(
            now,
            cancellationToken);

        if (next is null)
        {
            return recovery;
        }

        return next.Value < recovery
            ? next.Value
            : recovery;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Scheduled message worker failed; recovery will retry.")]
    private static partial void WorkerFailed(
        ILogger logger,
        Exception exception);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Scheduled message '{ScheduleId}' lost its claim.")]
    private static partial void ClaimLost(
        ILogger logger,
        Guid scheduleId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Scheduled message '{ScheduleId}' failed on attempt {Attempt}; retry in {Delay}.")]
    private static partial void MessageRetry(
        ILogger logger,
        Exception exception,
        Guid scheduleId,
        int attempt,
        TimeSpan delay);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Scheduled message '{ScheduleId}' failed permanently on attempt {Attempt}.")]
    private static partial void MessageFailed(
        ILogger logger,
        Exception exception,
        Guid scheduleId,
        int attempt);
}

using Clean.Messaging.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Clean.Messaging.Processing;

internal sealed partial class WorkLoop<TEntry>(
    IServiceScopeFactory scopes,
    TimeProvider time,
    ILogger<WorkLoop<TEntry>> logger)
    where TEntry : class, IDurableEntry
{
    public async Task Run(
        IWorkSignal signal,
        WorkLoopOptions options,
        CancellationToken cancellationToken)
    {
        options.Validate();

        var channel = Channel.CreateBounded<OwnedEntry<TEntry>>(
            new BoundedChannelOptions(options.Capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = true,
                AllowSynchronousContinuations = false
            });

        var slots = new SemaphoreSlim(
            options.Capacity + options.Concurrency);

        try
        {
            var tasks = new Task[options.Concurrency + 1];

            tasks[0] = Produce(
                signal,
                options,
                channel.Writer,
                slots,
                cancellationToken);

            for (var i = 0; i < options.Concurrency; i++)
            {
                tasks[i + 1] = Consume(
                    signal,
                    channel.Reader,
                    slots,
                    cancellationToken);
            }

            signal.Wake();

            await Task.WhenAll(tasks);
        }
        finally
        {
            slots.Dispose();
        }
    }

    private async Task Produce(
        IWorkSignal signal,
        WorkLoopOptions options,
        ChannelWriter<OwnedEntry<TEntry>> writer,
        SemaphoreSlim slots,
        CancellationToken cancellationToken)
    {
        var failures = 0;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await signal.WaitAsync(cancellationToken);

                var reserved = 0;

                try
                {
                    reserved = await Reserve(
                        slots,
                        options.BatchSize,
                        cancellationToken);

                    var owned = await Claim(
                        reserved,
                        options.Lease,
                        cancellationToken);

                    failures = 0;

                    var unused = reserved - owned.Count;

                    if (unused > 0)
                    {
                        slots.Release(unused);
                        reserved -= unused;
                    }

                    foreach (var entry in owned)
                    {
                        await writer.WriteAsync(
                            entry,
                            cancellationToken);

                        reserved--;
                    }

                    if (owned.Count == options.BatchSize)
                    {
                        signal.Wake();
                    }
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    failures++;

                    ClaimFailed(
                        logger,
                        exception);

                    await Task.Delay(
                        RecoveryDelay(
                            failures,
                            options),
                        time,
                        cancellationToken);

                    signal.Wake();
                }
                finally
                {
                    if (reserved > 0)
                    {
                        slots.Release(reserved);
                    }
                }
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            writer.TryComplete();
        }
    }

    private static async ValueTask<int> Reserve(
        SemaphoreSlim slots,
        int limit,
        CancellationToken cancellationToken)
    {
        await slots.WaitAsync(cancellationToken);

        var reserved = 1;

        while (reserved < limit &&
               slots.Wait(0, cancellationToken))
        {
            reserved++;
        }

        return reserved;
    }

    private async ValueTask<IReadOnlyList<OwnedEntry<TEntry>>> Claim(
        int limit,
        TimeSpan lease,
        CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();

        var store = scope.ServiceProvider
            .GetRequiredService<IDurableStore<TEntry>>();

        return await store.Claim(
            time.GetUtcNow().UtcDateTime,
            limit,
            lease,
            cancellationToken);
    }

    private async Task Consume(
        IWorkSignal signal,
        ChannelReader<OwnedEntry<TEntry>> reader,
        SemaphoreSlim slots,
        CancellationToken cancellationToken)
    {
        await foreach (var owned in reader.ReadAllAsync(
                           cancellationToken))
        {
            try
            {
                await Process(
                    owned,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                ProcessingFailed(
                    logger,
                    owned.Entry.MessageId,
                    owned.Entry.TargetId,
                    exception);
            }
            finally
            {
                slots.Release();
                signal.Wake();
            }
        }
    }

    private async ValueTask Process(
        OwnedEntry<TEntry> owned,
        CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();

        var processor = scope.ServiceProvider
            .GetRequiredService<IWorkProcessor<TEntry>>();

        await processor.Process(
            owned,
            cancellationToken);
    }

    private static TimeSpan RecoveryDelay(
        int failures,
        WorkLoopOptions options)
    {
        var exponential =
            options.RecoveryInterval.TotalMilliseconds *
            Math.Pow(
                2,
                Math.Min(failures - 1, 30));

        var maximum = Math.Min(
            options.FailureMaxDelay.TotalMilliseconds,
            exponential);

        return TimeSpan.FromMilliseconds(maximum);
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Work claiming failed; durable recovery will retry.")]
    private static partial void ClaimFailed(
        ILogger logger,
        Exception exception);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Work processing failed for {MessageId}/{TargetId}; durable recovery will retry.")]
    private static partial void ProcessingFailed(
        ILogger logger,
        Guid messageId,
        string targetId,
        Exception exception);
}

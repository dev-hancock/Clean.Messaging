using Clean.Messaging.Processing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Outbox;

public sealed class OutboxWorker(
    WorkLoop<OutboxEntry> loop,
    OutboxSignal signal,
    IOptions<OutboxOptions> options)
    : BackgroundService
{
    protected override Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var value = options.Value;

        return loop.Run(
            signal,
            new(
                value.Processing.Capacity,
                value.Processing.Concurrency,
                value.Processing.BatchSize,
                value.Lease.Duration,
                value.Recovery.Interval,
                value.Recovery.FailureMaxDelay),
            stoppingToken);
    }
}

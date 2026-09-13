using Clean.Messaging.Processing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Inbox;

public sealed class InboxWorker(
    WorkLoop<InboxEntry> loop,
    InboxSignal signal,
    IOptions<InboxOptions> options)
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

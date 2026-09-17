using Clean.Messaging.Inbox.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Inbox.Runtime;

public sealed class InboxRecoveryWorker(
    InboxSignal signal,
    TimeProvider time,
    IOptions<InboxOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            options.Value.Recovery.Interval,
            time);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                signal.Wake();
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
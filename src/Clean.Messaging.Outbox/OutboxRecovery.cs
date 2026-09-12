using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Outbox;

internal sealed class OutboxRecovery(
    OutboxSignal signal,
    TimeProvider time,
    IOptions<OutboxOptions> options) : BackgroundService
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

using System.Threading.Channels;

namespace Clean.Messaging.Scheduling;

internal sealed class SchedulingEngine(
    TimeProvider time)
    : ISchedulingEngine
{
    private readonly Channel<byte> _wake =
        Channel.CreateBounded<byte>(
            new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = false,
                SingleWriter = false
            });

    public void Wake()
    {
        _wake.Writer.TryWrite(0);
    }

    public async ValueTask Wait(
        DateTime wakeAt,
        CancellationToken cancellationToken)
    {
        var delay = wakeAt - time.GetUtcNow().UtcDateTime;

        if (delay <= TimeSpan.Zero)
        {
            return;
        }

        using var wait = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);

        var signalled = _wake.Reader
            .ReadAsync(wait.Token)
            .AsTask();

        var elapsed = Task.Delay(
            delay,
            time,
            wait.Token);

        try
        {
            var completed = await Task.WhenAny(
                signalled,
                elapsed);

            await completed;
        }
        finally
        {
            await wait.CancelAsync();

            try
            {
                await Task.WhenAll(
                    signalled,
                    elapsed);
            }
            catch (OperationCanceledException)
                when (wait.IsCancellationRequested)
            {
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        while (_wake.Reader.TryRead(out _))
        {
        }
    }
}

using System.Threading.Channels;

namespace Clean.Messaging.Processing;

public sealed class AsyncAutoResetEvent
{
    private readonly Channel<byte> _channel =
        Channel.CreateBounded<byte>(
            new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = false,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });

    public async ValueTask WaitAsync(
        CancellationToken cancellationToken = default)
    {
        await _channel.Reader.ReadAsync(
            cancellationToken);
    }

    public void Set()
    {
        _channel.Writer.TryWrite(0);
    }
}

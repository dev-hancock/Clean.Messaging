using Clean.Messaging.Processing;

namespace Clean.Messaging.Outbox.Runtime;

public sealed class OutboxSignal : IWorkSignal
{
    private readonly AsyncAutoResetEvent _signal = new();

    public ValueTask WaitAsync(
        CancellationToken cancellationToken)
    {
        return _signal.WaitAsync(
            cancellationToken);
    }

    public void Wake()
    {
        _signal.Set();
    }
}

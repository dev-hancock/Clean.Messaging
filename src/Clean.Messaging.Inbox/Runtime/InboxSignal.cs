using Clean.Messaging.Processing;

namespace Clean.Messaging.Inbox.Runtime;

public sealed class InboxSignal : IWorkSignal
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
using Clean.Messaging.Transport;

namespace Clean.Messaging.Outbox.Dispatch;

internal sealed class TransportMapper<TMessage>
    : ITransportMapper<TMessage, TMessage>
    where TMessage : notnull
{
    public TMessage Map(
        TMessage source)
    {
        return source;
    }
}
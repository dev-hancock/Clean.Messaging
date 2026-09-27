using Clean.Messaging.Transport;

namespace Clean.Messaging.Inbox.Ingress;

public interface ITransportMapper<out TMessage>
    where TMessage : notnull
{
    TMessage Map(
        TransportMessage transport);
}

internal sealed class TransportMapper<TMessage>(
    ITransportSerializer serializer)
    : ITransportMapper<TMessage>
    where TMessage : notnull
{
    public TMessage Map(
        TransportMessage transport)
    {
        return serializer.Deserialize<TMessage>(
            transport.Payload);
    }
}
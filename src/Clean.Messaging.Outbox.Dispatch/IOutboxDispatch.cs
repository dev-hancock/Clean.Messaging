using Clean.Messaging.Outbox.Transport;
using Clean.Messaging.Transport;

namespace Clean.Messaging.Outbox.Dispatch;

public interface IOutboxDispatch
{
    TransportMessage Create(
        DispatchMap map,
        OutboxEnvelope envelope);
}

internal sealed class OutboxDispatch(
    TransportDispatchRegistry registry)
    : IOutboxDispatch
{
    public TransportMessage Create(
        DispatchMap map,
        OutboxEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(
            envelope);

        return registry
            .Get(map)
            .Create(
                envelope);
    }
}

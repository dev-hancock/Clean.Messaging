using Clean.Messaging.Transport;

namespace Clean.Messaging.Inbox.Ingress;

public readonly record struct IngressMap(
    Type PayloadType,
    Type MessageType);

public interface IInboxIngress
{
    ValueTask Accept(
        IngressMap map,
        TransportMessage message,
        CancellationToken cancellationToken = default);
}

internal sealed class InboxIngress(
    TransportIngressRegistry registry)
    : IInboxIngress
{
    public ValueTask Accept(
        IngressMap map,
        TransportMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            message);

        return registry
            .Get(map)
            .Accept(
                message,
                cancellationToken);
    }
}
using System.Collections.Frozen;

namespace Clean.Messaging.Inbox.Ingress;

internal sealed class TransportIngressRegistry(
    IEnumerable<ITransportIngress> ingress)
{
    private readonly FrozenDictionary<
        IngressMap,
        ITransportIngress> _ingress =
        ingress.ToFrozenDictionary(
            registration => registration.Map);

    public ITransportIngress Get(
        IngressMap map)
    {
        if (_ingress.TryGetValue(
                map,
                out var ingress))
        {
            return ingress;
        }

        throw new InboxIngressException(
            $"No ingress mapping is registered from " +
            $"'{map.PayloadType}' to '{map.MessageType}'.");
    }
}
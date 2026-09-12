using Clean.Messaging.Persistence;
using System.Collections.Frozen;

namespace Clean.Messaging.Outbox;

internal sealed class OutboxDispatcher(
    IEnumerable<IOutboxDelivery> deliveries)
    : IOutboxDispatcher
{
    private readonly FrozenDictionary<string, IOutboxDelivery> _deliveries =
        Build(deliveries);

    public ValueTask<bool> Dispatch(
        OwnedEntry<OutboxEntry> owned,
        CancellationToken cancellationToken)
    {
        var transport =
            owned.Entry.Transport;

        if (_deliveries.TryGetValue(
                transport,
                out var delivery))
        {
            return delivery.Dispatch(
                owned,
                cancellationToken);
        }

        throw new InvalidOperationException(
            $"No outbox delivery is registered for transport '{transport}'.");
    }

    private static FrozenDictionary<string, IOutboxDelivery> Build(
        IEnumerable<IOutboxDelivery> deliveries)
    {
        var result =
            new Dictionary<string, IOutboxDelivery>(
                StringComparer.Ordinal);

        foreach (var delivery in deliveries)
        {
            if (!result.TryAdd(
                    delivery.Transport,
                    delivery))
            {
                throw new InvalidOperationException(
                    $"Outbox transport '{delivery.Transport}' is registered more than once.");
            }
        }

        return result.ToFrozenDictionary(
            StringComparer.Ordinal);
    }
}
using System.Collections.Frozen;

namespace Clean.Messaging.Outbox.Dispatch;

internal sealed class TransportDispatchRegistry(
    IEnumerable<ITransportDispatch> dispatches)
{
    private readonly FrozenDictionary<
        DispatchMap,
        ITransportDispatch> _dispatches =
        Create(
            dispatches);

    private static FrozenDictionary<
        DispatchMap,
        ITransportDispatch> Create(
        IEnumerable<ITransportDispatch> dispatches)
    {
        var registrations =
            dispatches.ToArray();

        var duplicate =
            registrations
                .GroupBy(registration => registration.Map)
                .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new OutboxDispatchException(
                $"More than one dispatch mapping is registered from " +
                $"'{duplicate.Key.MessageType}' to '{duplicate.Key.PayloadType}'.");
        }

        return registrations.ToFrozenDictionary(
            registration => registration.Map);
    }

    public ITransportDispatch Get(
        DispatchMap map)
    {
        if (_dispatches.TryGetValue(
                map,
                out var dispatch))
        {
            return dispatch;
        }

        throw new OutboxDispatchException(
            $"No dispatch mapping is registered from " +
            $"'{map.MessageType}' to '{map.PayloadType}'.");
    }
}
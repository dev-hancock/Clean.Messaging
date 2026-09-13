using System.Collections.Frozen;

namespace Clean.Messaging.Scheduling;

internal sealed class ScheduledMessageDeliveryRegistry
{
    private readonly FrozenDictionary<string, IScheduledMessageDelivery> _deliveries;

    public ScheduledMessageDeliveryRegistry(
        IEnumerable<IScheduledMessageDelivery> deliveries)
    {
        var byTarget = new Dictionary<string, IScheduledMessageDelivery>(
            StringComparer.Ordinal);

        foreach (var delivery in deliveries)
        {
            if (!byTarget.TryAdd(
                    delivery.Target.Value,
                    delivery))
            {
                throw new InvalidOperationException(
                    $"Scheduled message target '{delivery.Target}' is registered more than once.");
            }
        }

        _deliveries = byTarget.ToFrozenDictionary(
            StringComparer.Ordinal);
    }

    public IScheduledMessageDelivery Get(
        ScheduledMessageTarget target)
    {
        if (_deliveries.TryGetValue(
                target.Value,
                out var delivery))
        {
            return delivery;
        }

        throw new InvalidOperationException(
            $"Scheduled message target '{target}' is not registered.");
    }
}

using Clean.Messaging.Exceptions;
using System.Collections.Frozen;

namespace Clean.Messaging.Scheduling.Delivery;

internal sealed class ScheduleDeliveryRegistry
{
    private readonly FrozenDictionary<string, IScheduleDelivery> _deliveries;

    public ScheduleDeliveryRegistry(
        IEnumerable<IScheduleDelivery> deliveries)
    {
        var byTarget = new Dictionary<string, IScheduleDelivery>(
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

    public IScheduleDelivery Get(
        ScheduleTarget target)
    {
        if (_deliveries.TryGetValue(
                target.Value,
                out var delivery))
        {
            return delivery;
        }

        throw new MessageException(
            "scheduling.delivery.target_not_registered",
            $"Scheduled message target '{target}' is not registered.",
            FailureAction.Fault);
    }
}
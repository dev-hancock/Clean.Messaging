using System.Collections.Frozen;

namespace Clean.Messaging.Outbox.Mqtt.Routing;

internal sealed class MqttRouteRegistry(
    IEnumerable<MqttOutboxRoute> routes)
{
    private readonly FrozenDictionary<string, MqttOutboxRoute> _routes =
        routes.ToFrozenDictionary(
            route => route.Id,
            StringComparer.Ordinal);

    public MqttOutboxRoute Get(string targetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            targetId);

        if (_routes.TryGetValue(
                targetId,
                out var route))
        {
            return route;
        }

        throw new InvalidOperationException(
            $"MQTT route '{targetId}' is not registered.");
    }
}
using System.Collections.Frozen;

namespace Clean.Messaging.Outbox.Mqtt.Routing;

internal sealed class MqttRouteRegistry(
    IEnumerable<MqttRoute> routes)
{
    private readonly FrozenDictionary<string, MqttRoute> _routes =
        routes.ToFrozenDictionary(
            route => route.Id,
            StringComparer.Ordinal);

    public MqttRoute Get(string targetId)
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
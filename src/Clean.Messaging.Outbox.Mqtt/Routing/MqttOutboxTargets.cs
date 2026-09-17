using Clean.Messaging.Outbox.Mqtt.Publishing;
using Clean.Messaging.Outbox.Transport;

namespace Clean.Messaging.Outbox.Mqtt.Routing;

internal sealed class MqttOutboxTargets(
    IEnumerable<MqttRoute> routes)
    : IOutboxTargetProvider
{
    private readonly MqttRoute[] _routes =
        routes.ToArray();

    public IEnumerable<OutboxTarget> GetTargets(
        Type messageType,
        object message)
    {
        foreach (var route in _routes)
        {
            if (route.MessageType != messageType)
            {
                continue;
            }

            yield return new(
                route.Id,
                MqttTransport.Name,
                route.Destination(message));
        }
    }
}
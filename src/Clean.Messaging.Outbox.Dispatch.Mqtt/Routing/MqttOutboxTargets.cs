using Clean.Messaging.Outbox.Dispatch;
using Clean.Messaging.Outbox.Dispatch.Mqtt.Publishing;
using Clean.Messaging.Outbox.Transport;

namespace Clean.Messaging.Outbox.Dispatch.Mqtt.Routing;

internal sealed class MqttOutboxTargets(
    IEnumerable<MqttOutboxRoute> routes)
    : IOutboxTargetProvider
{
    private readonly MqttOutboxRoute[] _routes =
        routes.ToArray();

    public IEnumerable<OutboxTarget> GetTargets(
        Type messageType,
        object message)
    {
        foreach (var route in _routes)
        {
            if (route.Map.MessageType != messageType)
            {
                continue;
            }

            var destination =
                route.Destination(message);

            if (string.IsNullOrWhiteSpace(destination))
            {
                throw new OutboxDispatchException(
                    $"MQTT route '{route.Id}' produced no destination.");
            }

            yield return new(
                route.Id,
                MqttTransport.Name,
                destination);
        }
    }
}

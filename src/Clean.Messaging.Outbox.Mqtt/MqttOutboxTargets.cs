using Microsoft.Extensions.Options;

namespace Clean.Messaging.Outbox.Mqtt;

internal sealed class MqttOutboxTargets(
    IOptions<MqttOutboxOptions> options)
    : IOutboxTargetProvider
{
    public IEnumerable<OutboxTarget> GetTargets(
        Type messageType,
        object message)
    {
        foreach (var route in options.Value.Routes)
        {
            if (route.MessageType != messageType)
            {
                continue;
            }

            var destination =
                route.Destination(message);

            if (string.IsNullOrWhiteSpace(
                    destination))
            {
                throw new InvalidOperationException(
                    $"MQTT route '{route.Id}' produced an empty destination.");
            }

            yield return new(
                TargetId: route.Id,
                Transport: MqttTransport.Name,
                Destination: destination);
        }
    }
}
using MQTTnet;

namespace Clean.Messaging.Outbox.Mqtt;

internal sealed class MqttPublishException(
    MqttClientPublishResult result)
    : Exception(CreateMessage(result))
{
    private static string CreateMessage(
        MqttClientPublishResult result)
    {
        var reason = string.IsNullOrWhiteSpace(result.ReasonString)
            ? result.ReasonCode.ToString()
            : $"{result.ReasonCode}: {result.ReasonString}";

        return $"MQTT publish was rejected by the broker ({reason}).";
    }
}

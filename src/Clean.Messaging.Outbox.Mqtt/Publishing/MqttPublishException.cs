using Clean.Messaging.Exceptions;
using MQTTnet;

namespace Clean.Messaging.Outbox.Mqtt.Publishing;

internal sealed class MqttPublishException(
    MqttClientPublishResult result)
    : MessageException(
        GetCode(result.ReasonCode),
        CreateMessage(result),
        GetAction(result.ReasonCode))
{
    private static string CreateMessage(
        MqttClientPublishResult result)
    {
        var reason = string.IsNullOrWhiteSpace(result.ReasonString)
            ? result.ReasonCode.ToString()
            : $"{result.ReasonCode}: {result.ReasonString}";

        return $"MQTT publish was rejected by the broker ({reason}).";
    }

    private static FailureAction GetAction(
        MqttClientPublishReasonCode reasonCode) =>
        reasonCode is MqttClientPublishReasonCode.NotAuthorized or
            MqttClientPublishReasonCode.TopicNameInvalid or
            MqttClientPublishReasonCode.PayloadFormatInvalid
                ? FailureAction.Fault
                : FailureAction.Retry;

    private static string GetCode(
        MqttClientPublishReasonCode reasonCode) =>
        reasonCode switch
        {
            MqttClientPublishReasonCode.NotAuthorized
                => "mqtt.publish.not_authorized",
            MqttClientPublishReasonCode.TopicNameInvalid
                => "mqtt.publish.topic_name_invalid",
            MqttClientPublishReasonCode.PayloadFormatInvalid
                => "mqtt.publish.payload_format_invalid",
            _ => "mqtt.publish.rejected"
        };
}
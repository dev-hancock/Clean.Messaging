using MQTTnet;
using System.Buffers;
using System.Text.Json;

namespace Clean.Messaging.Inbox.Mqtt.Serialization;

internal sealed class JsonMqttMessageSerializer(
    JsonSerializerOptions options)
    : IMqttMessageSerializer
{
    public TMessage Deserialize<TMessage>(
        MqttApplicationMessage message)
        where TMessage : notnull
    {
        var value = JsonSerializer.Deserialize<TMessage>(
            message.Payload.ToArray(),
            options);

        return value ?? throw new InvalidOperationException(
            $"MQTT payload on topic '{message.Topic}' deserialized to null for '{typeof(TMessage)}'.");
    }
}
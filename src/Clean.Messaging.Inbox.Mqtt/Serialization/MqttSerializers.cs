using System.Text.Json;

namespace Clean.Messaging.Inbox.Mqtt.Serialization;

internal class MqttSerializers
{
    public static IMqttMessageSerializer Json { get; } =
        new JsonMqttMessageSerializer(
            new(JsonSerializerDefaults.Web));
}
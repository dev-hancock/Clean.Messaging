using MQTTnet;

namespace Clean.Messaging.Inbox.Mqtt.Serialization;

public interface IMqttMessageSerializer
{
    TMessage Deserialize<TMessage>(
        MqttApplicationMessage message)
        where TMessage : notnull;
}
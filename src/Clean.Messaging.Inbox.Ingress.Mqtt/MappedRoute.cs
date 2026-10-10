using Clean.Messaging.Transport;

namespace Clean.Messaging.Inbox.Ingress.Mqtt;

public sealed class MappedRoute<
    TPayload,
    TMessage,
    TMapper>
    where TPayload : notnull
    where TMessage : notnull
    where TMapper :
        class,
        ITransportMapper<TPayload, TMessage>
{
    private readonly MqttInboxBuilder _mqtt;
    private readonly string _topic;

    internal MappedRoute(
        MqttInboxBuilder mqtt,
        string topic)
    {
        _mqtt = mqtt;
        _topic = topic;
    }

    public MqttInboxBuilder Subscribe()
    {
        _mqtt.Subscribe<
            TPayload,
            TMessage,
            TMapper>(
            _topic);

        return _mqtt;
    }
}
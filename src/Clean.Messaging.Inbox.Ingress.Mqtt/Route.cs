using Clean.Messaging.Transport;

namespace Clean.Messaging.Inbox.Ingress.Mqtt;

public sealed class Route<TPayload>
    where TPayload : notnull
{
    private readonly MqttInboxBuilder _mqtt;
    private readonly string _topic;

    internal Route(
        MqttInboxBuilder mqtt,
        string topic)
    {
        _mqtt = mqtt;
        _topic = topic;
    }

    public MappedRoute<
        TPayload,
        TMessage,
        TMapper> Map<TMessage, TMapper>()
        where TMessage : notnull
        where TMapper :
            class,
            ITransportMapper<TPayload, TMessage>
    {
        return new(
            _mqtt,
            _topic);
    }

    public MqttInboxBuilder Subscribe()
    {
        _mqtt.Subscribe<TPayload>(
            _topic);

        return _mqtt;
    }
}
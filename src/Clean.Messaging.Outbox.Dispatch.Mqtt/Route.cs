using Clean.Messaging.Transport;

namespace Clean.Messaging.Outbox.Dispatch.Mqtt;

public sealed class Route<TMessage>
    where TMessage : notnull
{
    private readonly MqttOutboxBuilder _mqtt;
    private readonly string _id;

    internal Route(
        MqttOutboxBuilder mqtt,
        string id)
    {
        _mqtt = mqtt;
        _id = id;
    }

    public MappedRoute<
        TMessage,
        TPayload,
        TMapper> Map<TPayload, TMapper>()
        where TPayload : notnull
        where TMapper :
            class,
            ITransportMapper<TMessage, TPayload>
    {
        return new(
            _mqtt,
            _id);
    }

    public MqttOutboxBuilder Publish(
        Func<TMessage, string> destination,
        bool retain = false)
    {
        _mqtt.Publish(
            _id,
            destination,
            retain);

        return _mqtt;
    }
}

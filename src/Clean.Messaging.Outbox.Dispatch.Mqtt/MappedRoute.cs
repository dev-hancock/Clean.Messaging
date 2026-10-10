using Clean.Messaging.Transport;

namespace Clean.Messaging.Outbox.Dispatch.Mqtt;

public sealed class MappedRoute<
    TMessage,
    TPayload,
    TMapper>
    where TMessage : notnull
    where TPayload : notnull
    where TMapper :
        class,
        ITransportMapper<TMessage, TPayload>
{
    private readonly MqttOutboxBuilder _mqtt;
    private readonly string _id;

    internal MappedRoute(
        MqttOutboxBuilder mqtt,
        string id)
    {
        _mqtt = mqtt;
        _id = id;
    }

    public MqttOutboxBuilder Publish(
        Func<TMessage, string> destination,
        bool retain = false)
    {
        _mqtt.Publish<
            TMessage,
            TPayload,
            TMapper>(
            _id,
            destination,
            retain);

        return _mqtt;
    }
}

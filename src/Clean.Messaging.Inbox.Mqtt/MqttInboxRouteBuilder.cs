using MQTTnet;

namespace Clean.Messaging.Inbox.Mqtt;

public sealed class MqttInboxRouteBuilder<TMessage>
    where TMessage : notnull
{
    private readonly MqttInboxRoute<TMessage> _route;

    internal MqttInboxRouteBuilder(
        MqttInboxRoute<TMessage> route)
    {
        _route = route;
    }

    public MqttInboxRouteBuilder<TMessage> EventId(
        Func<MqttApplicationMessage, TMessage, Guid?> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        _route.EventId = selector;

        return this;
    }

    public MqttInboxRouteBuilder<TMessage> CorrelationId(
        Func<MqttApplicationMessage, TMessage, Guid?> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        _route.CorrelationId = selector;

        return this;
    }

    public MqttInboxRouteBuilder<TMessage> CausationId(
        Func<MqttApplicationMessage, TMessage, Guid?> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        _route.CausationId = selector;

        return this;
    }
}

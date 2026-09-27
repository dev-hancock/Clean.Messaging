namespace Clean.Messaging.Inbox.Ingress.Mqtt.Routing;

internal interface IMqttInboxRoute
{
    string TopicFilter { get; }

    IngressMap Map { get; }
}

internal sealed record MqttInboxRoute(
    string TopicFilter,
    IngressMap Map)
    : IMqttInboxRoute;
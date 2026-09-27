namespace Clean.Messaging.Inbox.Ingress.Mqtt.Routing;

internal static class MqttHeaders
{
    public const string MessageId =
        "message-id";

    public const string EventId =
        "event-id";

    public const string Contract =
        "contract";

    public const string CorrelationId =
        "correlation-id";

    public const string CausationId =
        "causation-id";
}
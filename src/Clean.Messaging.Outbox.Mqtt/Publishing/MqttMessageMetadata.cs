namespace Clean.Messaging.Outbox.Mqtt.Publishing;

public static class MqttMessageMetadata
{
    public const string MessageId = "MessageId";

    public const string EventId = "EventId";

    public const string TargetId = "TargetId";

    public const string Contract = "Contract";

    public const string CorrelationId = "CorrelationId";

    public const string CausationId = "CausationId";
}

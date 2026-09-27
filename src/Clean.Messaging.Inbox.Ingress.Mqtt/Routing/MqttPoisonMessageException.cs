namespace Clean.Messaging.Inbox.Ingress.Mqtt.Routing;

internal sealed class MqttPoisonMessageException
    : Exception
{
    public MqttPoisonMessageException(
        string message)
        : base(message)
    {
    }

    public MqttPoisonMessageException(
        string message,
        Exception innerException)
        : base(
            message,
            innerException)
    {
    }
}
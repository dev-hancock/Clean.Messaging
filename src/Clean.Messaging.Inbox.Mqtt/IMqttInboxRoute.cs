using MQTTnet;

namespace Clean.Messaging.Inbox.Mqtt;

internal interface IMqttInboxRoute
{
    string TopicFilter { get; }

    ValueTask Accept(
        IServiceProvider services,
        MqttApplicationMessage message,
        CancellationToken cancellationToken);
}
using MQTTnet;

namespace Clean.Messaging.Inbox.Mqtt;

internal sealed class MqttInboxRouter(
    IEnumerable<IMqttInboxRoute> routes)
{
    public IReadOnlyList<IMqttInboxRoute> Routes { get; } =
        routes.ToArray();

    public IMqttInboxRoute Resolve(
        string topic)
    {
        var matches = Routes
            .Where(route =>
                MqttTopicFilterComparer.Compare(
                    topic,
                    route.TopicFilter) ==
                MqttTopicFilterCompareResult.IsMatch)
            .ToArray();

        return matches.Length switch
        {
            1 => matches[0],

            0 => throw new MqttInboxPoisonMessageException(
                $"No MQTT inbox route matches topic '{topic}'."),

            _ => throw new MqttInboxPoisonMessageException(
                $"Multiple MQTT inbox routes match topic '{topic}'.")
        };
    }
}
using MQTTnet;

namespace Clean.Messaging.Inbox.Mqtt;

internal sealed class MqttInboxRouter
{
    public MqttInboxRouter(
        IEnumerable<IMqttInboxRoute> routes)
    {
        Routes = routes.ToArray();

        var filters = Routes
            .Select(route =>
                RouteFilter.Parse(
                    route.TopicFilter))
            .ToArray();

        for (var first = 0; first < filters.Length; first++)
        {
            for (var second = first + 1;
                second < filters.Length;
                second++)
            {
                if (!filters[first].Overlaps(
                        filters[second]))
                {
                    continue;
                }

                throw new InvalidOperationException(
                    $"MQTT inbox routes '{filters[first].Value}' and '{filters[second].Value}' overlap.");
            }
        }
    }

    public IReadOnlyList<IMqttInboxRoute> Routes { get; }

    public IMqttInboxRoute Resolve(
        string topic)
    {
        IMqttInboxRoute? match = null;

        foreach (var route in Routes)
        {
            if (MqttTopicFilterComparer.Compare(
                    topic,
                    route.TopicFilter) !=
                MqttTopicFilterCompareResult.IsMatch)
            {
                continue;
            }

            if (match is not null)
            {
                throw new MqttInboxPoisonMessageException(
                    $"Multiple MQTT inbox routes match topic '{topic}'.");
            }

            match = route;
        }

        return match ?? throw new MqttInboxPoisonMessageException(
            $"No MQTT inbox route matches topic '{topic}'.");
    }
}

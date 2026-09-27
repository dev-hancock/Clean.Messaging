using MQTTnet;

namespace Clean.Messaging.Inbox.Ingress.Mqtt.Routing;

internal sealed class MqttInboxRouter
{
    private readonly IMqttInboxRoute[] _routes;

    public MqttInboxRouter(
        IEnumerable<IMqttInboxRoute> routes)
    {
        _routes = routes.ToArray();

        Validate();
    }

    public IReadOnlyList<IMqttInboxRoute> Routes =>
        _routes;

    public IMqttInboxRoute Resolve(
        string topic)
    {
        IMqttInboxRoute? match = null;

        foreach (var route in _routes)
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
                throw new MqttPoisonMessageException(
                    $"Multiple MQTT inbox routes match topic '{topic}'.");
            }

            match = route;
        }

        return match ?? throw new MqttPoisonMessageException(
            $"No MQTT inbox route matches topic '{topic}'.");
    }

    private void Validate()
    {
        var filters = _routes
            .Select(route =>
                RouteFilter.Parse(
                    route.TopicFilter))
            .ToArray();

        for (var first = 0;
             first < filters.Length;
             first++)
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
                    $"MQTT inbox routes '{filters[first].Value}' " +
                    $"and '{filters[second].Value}' overlap.");
            }
        }
    }
}
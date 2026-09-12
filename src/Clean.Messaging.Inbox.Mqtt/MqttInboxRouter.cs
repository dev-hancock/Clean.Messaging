namespace Clean.Messaging.Inbox.Mqtt;

internal sealed class MqttInboxRouter(
    IEnumerable<IMqttInboxRoute> routes)
{
    private readonly IMqttInboxRoute[] _routes = routes.ToArray();

    public IReadOnlyList<IMqttInboxRoute> Routes => _routes;

    public IMqttInboxRoute Resolve(
        string topic)
    {
        var matches = _routes
            .Where(route => route.Matches(topic))
            .Take(2)
            .ToArray();

        return matches.Length switch
        {
            1 => matches[0],

            0 => throw new InvalidOperationException(
                $"No MQTT inbox route matches topic '{topic}'."),

            _ => throw new InvalidOperationException(
                $"More than one MQTT inbox route matches topic '{topic}'.")
        };
    }
}

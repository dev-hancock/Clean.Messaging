using MQTTnet.Protocol;

namespace Clean.Messaging.Outbox.Mqtt;

public sealed class MqttOutboxOptions
{
    private readonly List<MqttRoute> _routes = [];

    public MqttQualityOfServiceLevel QualityOfService { get; set; } =
        MqttQualityOfServiceLevel.AtLeastOnce;

    public bool Retain { get; set; }

    internal IReadOnlyList<MqttRoute> Routes =>
        _routes;

    public void Route<TMessage>(
        string id,
        Func<TMessage, string> destination)
        where TMessage : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            id);

        ArgumentNullException.ThrowIfNull(
            destination);

        if (id.Length > 200 ||
            id.Any(character => character > 127))
        {
            throw new ArgumentException(
                "Outbox target identities must be at most 200 ASCII characters.",
                nameof(id));
        }

        if (_routes.Any(route =>
                string.Equals(
                    route.Id,
                    id,
                    StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"MQTT outbox route '{id}' is already registered.");
        }

        _routes.Add(
            new(
                typeof(TMessage),
                id,
                message =>
                    destination(
                        (TMessage)message)));
    }
}

internal sealed record MqttRoute(
    Type MessageType,
    string Id,
    Func<object, string> Destination);
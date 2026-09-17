namespace Clean.Messaging.Outbox.Mqtt.Routing;

internal sealed record MqttRoute(
    Type MessageType,
    string Id,
    Func<object, string> Destination,
    bool Retain);
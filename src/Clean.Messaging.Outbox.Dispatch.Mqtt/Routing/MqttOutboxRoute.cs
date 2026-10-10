using Clean.Messaging.Outbox.Dispatch;

namespace Clean.Messaging.Outbox.Dispatch.Mqtt.Routing;

internal sealed record MqttOutboxRoute(
    string Id,
    DispatchMap Map,
    Func<object, string> Destination,
    bool Retain);
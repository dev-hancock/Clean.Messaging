using Clean.Messaging.Outbox.Mqtt.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Outbox.Mqtt;

public sealed class MqttOutboxBuilder
{
    private readonly IServiceCollection _services;
    private readonly HashSet<string> _routes = [];

    internal MqttOutboxBuilder(
        IServiceCollection services)
    {
        _services = services;
    }

    internal int RouteCount =>
        _routes.Count;

    public void Route<TMessage>(
        string id,
        Func<TMessage, string> destination,
        bool retain = false)
        where TMessage : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            id);

        ArgumentNullException.ThrowIfNull(
            destination);

        ValidateId(id);

        if (!_routes.Add(id))
        {
            throw new InvalidOperationException(
                $"MQTT outbox route '{id}' is already registered.");
        }

        _services.AddSingleton(
            new MqttRoute(
                typeof(TMessage),
                id,
                message =>
                    destination(
                        (TMessage)message),
                retain));
    }

    private static void ValidateId(
        string id)
    {
        if (id.Length <= 200 &&
            id.All(character =>
                character <= 127))
        {
            return;
        }

        throw new ArgumentException(
            "Outbox target identities must be at most 200 ASCII characters.",
            nameof(id));
    }
}
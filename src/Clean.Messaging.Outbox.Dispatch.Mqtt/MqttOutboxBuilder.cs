using Clean.Messaging.Outbox.Dispatch.Mqtt.Routing;
using Clean.Messaging.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Outbox.Dispatch.Mqtt;

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

    public Route<TMessage> On<TMessage>(
        string id)
        where TMessage : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            id);

        ValidateId(id);

        return new(
            this,
            id);
    }

    internal void Publish<TMessage>(
        string id,
        Func<TMessage, string> destination,
        bool retain)
        where TMessage : notnull
    {
        Register<
            TMessage,
            TMessage>(
            id,
            destination,
            retain);

        _services.AddDispatch<TMessage>();
    }

    internal void Publish<
        TMessage,
        TPayload,
        TMapper>(
        string id,
        Func<TMessage, string> destination,
        bool retain)
        where TMessage : notnull
        where TPayload : notnull
        where TMapper :
            class,
            ITransportMapper<TMessage, TPayload>
    {
        Register<
            TMessage,
            TPayload>(
            id,
            destination,
            retain);

        _services.AddDispatch<
            TMessage,
            TPayload,
            TMapper>();
    }

    private void Register<
        TMessage,
        TPayload>(
        string id,
        Func<TMessage, string> destination,
        bool retain)
        where TMessage : notnull
        where TPayload : notnull
    {
        ArgumentNullException.ThrowIfNull(
            destination);

        if (!_routes.Add(id))
        {
            throw new InvalidOperationException(
                $"MQTT outbox route '{id}' is already registered.");
        }

        _services.AddSingleton(
            new MqttOutboxRoute(
                id,
                new(
                    typeof(TMessage),
                    typeof(TPayload)),
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

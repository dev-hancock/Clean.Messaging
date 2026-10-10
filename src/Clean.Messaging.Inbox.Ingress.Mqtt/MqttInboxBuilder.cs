using Clean.Messaging.Inbox.Ingress.Mqtt.Routing;
using Clean.Messaging.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Inbox.Ingress.Mqtt;

public sealed class MqttInboxBuilder
{
    private readonly IServiceCollection _services;

    internal MqttInboxBuilder(
        IServiceCollection services)
    {
        _services = services;
    }

    internal int RouteCount { get; private set; }

    public Route<TPayload> On<TPayload>(
        string topicFilter)
        where TPayload : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            topicFilter);

        return new(
            this,
            topicFilter);
    }

    internal void Subscribe<
        TPayload,
        TMessage,
        TMapper>(
        string topicFilter)
        where TPayload : notnull
        where TMessage : notnull
        where TMapper :
            class,
            ITransportMapper<TPayload, TMessage>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            topicFilter);

        _services.AddIngress<
            TPayload,
            TMessage,
            TMapper>();

        _services.AddSingleton<IMqttInboxRoute>(
            new MqttInboxRoute(
                topicFilter,
                new(
                    typeof(TPayload),
                    typeof(TMessage))));

        RouteCount++;
    }

    internal void Subscribe<TMessage>(
        string topicFilter)
        where TMessage : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            topicFilter);

        _services.AddIngress<TMessage>();

        _services.AddSingleton<IMqttInboxRoute>(
            new MqttInboxRoute(
                topicFilter,
                new(
                    typeof(TMessage),
                    typeof(TMessage))));

        RouteCount++;
    }
}
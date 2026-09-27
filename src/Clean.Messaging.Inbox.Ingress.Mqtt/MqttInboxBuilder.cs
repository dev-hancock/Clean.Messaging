using Clean.Messaging.Inbox.Ingress.Mqtt.Routing;
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

    internal int Routes { get; private set; }

    public MqttInboxBuilder Subscribe<TMessage>(
        string topicFilter)
        where TMessage : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            topicFilter);

        _services.AddIngress<TMessage>();

        AddRoute(
            topicFilter,
            new(
                typeof(TMessage),
                typeof(TMessage)));

        return this;
    }

    public MqttInboxBuilder Subscribe<
        TPayload,
        TMessage,
        TMapper>(
        string topicFilter)
        where TPayload : notnull
        where TMessage : notnull
        where TMapper :
        class,
        ITransportMapper<TMessage>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            topicFilter);

        _services.AddIngress<
            TPayload,
            TMessage,
            TMapper>();

        AddRoute(
            topicFilter,
            new(
                typeof(TPayload),
                typeof(TMessage)));

        return this;
    }

    private void AddRoute(
        string topicFilter,
        IngressMap map)
    {
        _services.AddSingleton<IMqttInboxRoute>(
            new MqttInboxRoute(
                topicFilter,
                map));

        Routes++;
    }
}
using Clean.Messaging.Inbox.Mqtt.Routing;
using Clean.Messaging.Inbox.Mqtt.Serialization;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;

namespace Clean.Messaging.Inbox.Mqtt;

public sealed class MqttInboxBuilder
{
    private readonly IServiceCollection _services;

    internal MqttInboxBuilder(
        IServiceCollection services)
    {
        _services = services;
    }

    internal int RouteCount { get; private set; }

    public void Subscribe<TMessage>(
        string topicFilter,
        Func<
            MqttApplicationMessage,
            TMessage,
            InboxMessage<TMessage>> map)
        where TMessage : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            topicFilter);

        ArgumentNullException.ThrowIfNull(
            map);

        _services.AddSingleton<IMqttInboxRoute>(
            services =>
                new MqttInboxRoute<TMessage>(
                    topicFilter,
                    services.GetRequiredService<
                        IMqttMessageSerializer>(),
                    map));

        RouteCount++;
    }
}

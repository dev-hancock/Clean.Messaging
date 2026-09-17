using Clean.Messaging.Inbox.Mqtt.Routing;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using System.Buffers;
using System.Text.Json;

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
        Func<MqttApplicationMessage, TMessage> deserialize,
        Func<MqttApplicationMessage, TMessage, InboxMessage<TMessage>> map)
        where TMessage : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicFilter);
        ArgumentNullException.ThrowIfNull(deserialize);
        ArgumentNullException.ThrowIfNull(map);

        var route = new MqttInboxRoute<TMessage>(
            topicFilter,
            deserialize,
            map);

        _services.AddSingleton<IMqttInboxRoute>(route);
        RouteCount++;
    }

    public void SubscribeJson<TMessage>(
        string topicFilter,
        Func<MqttApplicationMessage, TMessage, InboxMessage<TMessage>> map,
        JsonSerializerOptions? json = null)
        where TMessage : notnull
    {
        var options = json ?? new JsonSerializerOptions(
            JsonSerializerDefaults.Web);

        Subscribe(
            topicFilter,
            message => Deserialize<TMessage>(
                message,
                options),
            map);
    }

    private static TMessage Deserialize<TMessage>(
        MqttApplicationMessage message,
        JsonSerializerOptions options)
        where TMessage : notnull
    {
        var value = JsonSerializer.Deserialize<TMessage>(
            message.Payload.ToArray(),
            options);

        return value ?? throw new InvalidOperationException(
            $"MQTT payload on topic '{message.Topic}' deserialized to null for '{typeof(TMessage)}'.");
    }
}

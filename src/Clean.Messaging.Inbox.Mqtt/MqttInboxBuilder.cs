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

    public MqttInboxRouteBuilder<TMessage> Subscribe<TMessage>(
        string topicFilter,
        Func<MqttApplicationMessage, TMessage> deserialize,
        Func<MqttApplicationMessage, TMessage, Guid> messageId)
        where TMessage : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicFilter);
        ArgumentNullException.ThrowIfNull(deserialize);
        ArgumentNullException.ThrowIfNull(messageId);

        var route = new MqttInboxRoute<TMessage>(
            topicFilter,
            deserialize,
            messageId);

        _services.AddSingleton<IMqttInboxRoute>(route);
        RouteCount++;

        return new(route);
    }

    public MqttInboxRouteBuilder<TMessage> SubscribeJson<TMessage>(
        string topicFilter,
        Func<MqttApplicationMessage, TMessage, Guid> messageId,
        JsonSerializerOptions? json = null)
        where TMessage : notnull
    {
        var options = json ?? new JsonSerializerOptions(
            JsonSerializerDefaults.Web);

        return Subscribe(
            topicFilter,
            message => Deserialize<TMessage>(
                message,
                options),
            messageId);
    }

    private static TMessage Deserialize<TMessage>(
        MqttApplicationMessage message,
        JsonSerializerOptions options)
        where TMessage : notnull
    {
        var payload = message.Payload.ToArray();

        var value = JsonSerializer.Deserialize<TMessage>(
            payload,
            options);

        return value ?? throw new InvalidOperationException(
            $"MQTT payload on topic '{message.Topic}' deserialized to null for '{typeof(TMessage)}'.");
    }
}

using Clean.Messaging.Outbox.Dispatch;
using Clean.Messaging.Outbox.Dispatch.Mqtt.Routing;
using Clean.Messaging.Transport;
using MQTTnet;
using System.Text;

namespace Clean.Messaging.Outbox.Dispatch.Mqtt.Publishing;

internal static class MqttMessageFactory
{
    public static MqttApplicationMessage Create(
        string? destination,
        TransportMessage transport,
        MqttOutboxRoute route,
        MqttOutboxOptions options)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new OutboxDispatchException(
                $"MQTT route '{route.Id}' has no destination.");
        }

        ArgumentNullException.ThrowIfNull(
            options);

        var builder =
            new MqttApplicationMessageBuilder()
                .WithTopic(destination)
                .WithPayloadSegment(transport.Payload)
                .WithQualityOfServiceLevel(
                    options.QualityOfService)
                .WithRetainFlag(
                    route.Retain)
                .WithUserProperty(
                    MqttHeaders.MessageId,
                    Encode(
                        GetRequiredId(
                            transport.MessageId,
                            MqttHeaders.MessageId)))
                .WithUserProperty(
                    MqttHeaders.EventId,
                    Encode(
                        GetRequiredId(
                            transport.EventId,
                            MqttHeaders.EventId)))
                .WithUserProperty(
                    MqttHeaders.Contract,
                    Encode(transport.Contract));

        if (!string.IsNullOrWhiteSpace(transport.ContentType))
        {
            builder.WithContentType(
                transport.ContentType);
        }

        if (transport.CorrelationId is { } correlationId)
        {
            builder.WithUserProperty(
                MqttHeaders.CorrelationId,
                Encode(
                    correlationId.ToString("D")));
        }

        if (transport.CausationId is { } causationId)
        {
            builder.WithUserProperty(
                MqttHeaders.CausationId,
                Encode(
                    causationId.ToString("D")));
        }

        foreach (var header in transport.Headers)
        {
            if (IsReservedHeader(
                    header.Name))
            {
                continue;
            }

            builder.WithUserProperty(
                header.Name,
                Encode(header.Value));
        }

        return builder.Build();
    }

    private static string GetRequiredId(
        Guid? value,
        string header) =>
        value is { } id && id != Guid.Empty
            ? id.ToString("D")
            : throw new OutboxDispatchException(
                $"MQTT outbound message has no valid '{header}' identifier.");

    private static bool IsReservedHeader(
        string name) =>
        name is MqttHeaders.MessageId or
            MqttHeaders.EventId or
            MqttHeaders.Contract or
            MqttHeaders.CorrelationId or
            MqttHeaders.CausationId;

    private static ReadOnlyMemory<byte> Encode(
        string value) =>
        Encoding.UTF8.GetBytes(value);
}

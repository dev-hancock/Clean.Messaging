using MQTTnet;
using System.Text;

namespace Clean.Messaging.Outbox.Mqtt;

internal static class MqttMessageFactory
{
    public static MqttApplicationMessage Create(
        OutboxDispatch message,
        MqttOutboxOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            message.Destination);

        ArgumentNullException.ThrowIfNull(
            options);

        var builder =
            new MqttApplicationMessageBuilder()
                .WithTopic(message.Destination)
                .WithPayloadSegment(message.Payload)
                .WithQualityOfServiceLevel(
                    options.QualityOfService)
                .WithRetainFlag(
                    options.Retain)
                .WithUserProperty(
                    MqttMessageMetadata.MessageId,
                    Encode(
                        message.MessageId.ToString("D")))
                .WithUserProperty(
                    MqttMessageMetadata.EventId,
                    Encode(
                        message.EventId.ToString("D")))
                .WithUserProperty(
                    MqttMessageMetadata.TargetId,
                    Encode(message.TargetId))
                .WithUserProperty(
                    MqttMessageMetadata.Contract,
                    Encode(message.Contract));

        if (message.CorrelationId is { } correlationId)
        {
            builder.WithUserProperty(
                MqttMessageMetadata.CorrelationId,
                Encode(
                    correlationId.ToString("D")));
        }

        if (message.CausationId is { } causationId)
        {
            builder.WithUserProperty(
                MqttMessageMetadata.CausationId,
                Encode(
                    causationId.ToString("D")));
        }

        return builder.Build();
    }

    private static ReadOnlyMemory<byte> Encode(
        string value) =>
        Encoding.UTF8.GetBytes(value);
}

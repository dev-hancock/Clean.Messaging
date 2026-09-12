using Microsoft.Extensions.Options;
using MQTTnet;

namespace Clean.Messaging.Outbox.Mqtt;

internal sealed class MqttOutboxTransport(
    MqttConnection connection,
    IOptions<MqttOutboxOptions> options)
    : IOutboxTransport
{
    public string Name =>
        MqttTransport.Name;

    public async ValueTask Dispatch(
        OutboxDispatch message,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            message.Destination);

        var client = await connection.GetClient(
            cancellationToken);

        var applicationMessage =
            new MqttApplicationMessageBuilder()
                .WithTopic(message.Destination)
                .WithPayloadSegment(message.Payload)
                .WithQualityOfServiceLevel(
                    options.Value.QualityOfService)
                .WithRetainFlag(
                    options.Value.Retain)
                .Build();

        var result = await client.PublishAsync(
            applicationMessage,
            cancellationToken);

        if (!result.IsSuccess)
        {
            throw new MqttPublishException(
                result);
        }
    }
}
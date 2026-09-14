using Microsoft.Extensions.Options;

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
        var client = await connection.GetClient(
            cancellationToken);

        var applicationMessage =
            MqttMessageFactory.Create(
                message,
                options.Value);

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
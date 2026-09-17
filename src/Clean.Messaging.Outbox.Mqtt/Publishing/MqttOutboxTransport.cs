using Clean.Messaging.Outbox.Mqtt.Connection;
using Clean.Messaging.Outbox.Mqtt.Routing;
using Clean.Messaging.Outbox.Transport;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Outbox.Mqtt.Publishing;

internal sealed class MqttOutboxTransport(
    MqttConnection connection,
    MqttRouteRegistry routes,
    IOptions<MqttOutboxOptions> options)
    : IOutboxTransport
{
    public string Name =>
        MqttTransport.Name;

    public async ValueTask Dispatch(
        OutboxDispatch dispatch,
        CancellationToken cancellationToken)
    {
        var route = routes.Get(
            dispatch.TargetId);

        var client = await connection.GetClient(
            cancellationToken);

        var message = MqttMessageFactory.Create(
            dispatch,
            route,
            options.Value);

        var result = await client.PublishAsync(
            message,
            cancellationToken);

        if (!result.IsSuccess)
        {
            throw new MqttPublishException(
                result);
        }
    }
}
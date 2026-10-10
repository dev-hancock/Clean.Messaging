using Clean.Messaging.Outbox.Dispatch.Mqtt.Connection;
using Clean.Messaging.Outbox.Dispatch.Mqtt.Routing;
using Clean.Messaging.Outbox.Transport;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Outbox.Dispatch.Mqtt.Publishing;

internal sealed class MqttOutboxTransport(
    MqttConnection connection,
    IOutboxDispatch dispatch,
    MqttRouteRegistry routes,
    IOptions<MqttOutboxOptions> options)
    : IOutboxTransport
{
    public string Name =>
        MqttTransport.Name;

    public async ValueTask Dispatch(
        OutboxEnvelope envelope,
        CancellationToken cancellationToken)
    {
        var route = routes.Get(
            envelope.TargetId);

        var transport = dispatch.Create(
            route.Map,
            envelope);

        var client = await connection.GetClient(
            cancellationToken);

        var mqttMessage = MqttMessageFactory.Create(
            envelope.Destination,
            transport,
            route,
            options.Value);

        var result = await client.PublishAsync(
            mqttMessage,
            cancellationToken);

        if (!result.IsSuccess)
        {
            throw new MqttPublishException(
                result);
        }
    }
}

using Clean.Messaging.Contracts;
using Clean.Messaging.Inbox.Ingress.Mqtt.Routing;
using Clean.Messaging.Transport;
using MQTTnet;
using MQTTnet.Packets;
using System.Buffers;

namespace Clean.Messaging.Inbox.Ingress.Mqtt;

internal sealed class MqttInboxIngressHandler(
    IMessageContractRegistry contracts,
    IInboxIngress ingress)
{
    public ValueTask Accept(
        IMqttInboxRoute route,
        MqttApplicationMessage message,
        CancellationToken cancellationToken)
    {
        var transport = Map(
            route,
            message);

        return ingress.Accept(
            route.Map,
            transport,
            cancellationToken);
    }

    private TransportMessage Map(
        IMqttInboxRoute route,
        MqttApplicationMessage message)
    {
        var contract = contracts.GetContract(
            route.Map.MessageType);

        var declaredContract = Get(
            message,
            MqttHeaders.Contract);

        if (declaredContract is not null &&
            !string.Equals(
                declaredContract,
                contract,
                StringComparison.Ordinal))
        {
            throw new MqttPoisonMessageException(
                $"MQTT message on topic '{message.Topic}' declared contract " +
                $"'{declaredContract}' but route expects '{contract}'.");
        }

        return new()
        {
            MessageId = GetGuid(
                message,
                MqttHeaders.MessageId),

            EventId = GetGuid(
                message,
                MqttHeaders.EventId),

            Contract = contract,

            Payload = GetPayload(
                message),

            CorrelationId = GetGuid(
                message,
                MqttHeaders.CorrelationId),

            CausationId = GetGuid(
                message,
                MqttHeaders.CausationId),

            ContentType = message.ContentType,

            Headers = message.UserProperties
                .Select(property =>
                    new TransportHeader(
                        property.Name,
                        property.ReadValueAsString()))
                .ToArray()
        };
    }
    private static ReadOnlyMemory<byte> GetPayload(
        MqttApplicationMessage message)
    {
        return message.Payload.IsSingleSegment
            ? message.Payload.First
            : message.Payload.ToArray();
    }

    private static Guid? GetGuid(
        MqttApplicationMessage message,
        string name)
    {
        var value = Get(
            message,
            name);

        if (value is null)
        {
            return null;
        }

        if (Guid.TryParse(
                value,
                out var id) &&
            id != Guid.Empty)
        {
            return id;
        }

        throw new MqttPoisonMessageException(
            $"MQTT property '{name}' on topic '{message.Topic}' " +
            "does not contain a valid identifier.");
    }

    private static string? Get(
        MqttApplicationMessage message,
        string name)
    {
        for (var index =
                 message.UserProperties.Count - 1;
             index >= 0;
             index--)
        {
            var property =
                message.UserProperties[index];

            if (string.Equals(
                    property.Name,
                    name,
                    StringComparison.Ordinal))
            {
                return property.ReadValueAsString();
            }
        }

        return null;
    }
}
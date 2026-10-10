using Clean.Messaging.Contracts;
using Clean.Messaging.Outbox.Transport;
using Clean.Messaging.Transport;

namespace Clean.Messaging.Outbox.Dispatch;

internal sealed class TransportMessageFactory(
    IMessageContractRegistry contracts,
    ITransportSerializer serializer)
{
    public TransportMessage Create<TPayload>(
        OutboxEnvelope envelope,
        TPayload payload)
        where TPayload : notnull
    {
        return new()
        {
            MessageId = envelope.MessageId,
            EventId = envelope.EventId,

            Contract = contracts.GetContract(
                typeof(TPayload)),

            Payload = serializer.Serialize(
                payload),

            CorrelationId = envelope.CorrelationId,
            CausationId = envelope.CausationId
        };
    }
}

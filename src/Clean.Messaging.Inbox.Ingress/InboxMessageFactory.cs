using Clean.Messaging.Transport;

namespace Clean.Messaging.Inbox.Ingress;

internal sealed class InboxMessageFactory
{
    public InboxMessage<TMessage> Create<TMessage>(
        TransportMessage transport,
        TMessage message)
        where TMessage : notnull
    {
        if (transport.MessageId is not { } messageId ||
            messageId == Guid.Empty)
        {
            throw new InboxIngressException(
                $"Transport message '{transport.Contract}' has no message ID.");
        }

        return new(
            messageId,
            message,
            transport.CorrelationId,
            transport.CausationId,
            transport.EventId);
    }
}
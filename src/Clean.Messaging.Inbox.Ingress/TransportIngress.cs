using Clean.Messaging.Transport;

namespace Clean.Messaging.Inbox.Ingress;

internal interface ITransportIngress
{
    IngressMap Map { get; }

    ValueTask Accept(
        TransportMessage message,
        CancellationToken cancellationToken);
}

internal sealed class TransportIngress<
    TPayload,
    TMessage,
    TMapper>(
    ITransportSerializer serializer,
    TMapper mapper,
    InboxMessageFactory factory,
    IInbox inbox)
    : ITransportIngress
    where TPayload : notnull
    where TMessage : notnull
    where TMapper :
    class,
    ITransportMapper<TPayload, TMessage>
{
    public IngressMap Map { get; } =
        new(
            typeof(TPayload),
            typeof(TMessage));

    public async ValueTask Accept(
        TransportMessage transport,
        CancellationToken cancellationToken)
    {
        InboxMessage<TMessage> incoming;

        try
        {
            var payload =
                serializer.Deserialize<TPayload>(
                    transport.Payload);

            var message =
                mapper.Map(
                    payload);

            incoming =
                factory.Create(
                    transport,
                    message);
        }
        catch (InboxIngressException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new InboxIngressException(
                $"Transport message '{transport.Contract}' could not be mapped " +
                $"from '{typeof(TPayload)}' to '{typeof(TMessage)}'.",
                exception);
        }

        await inbox.Accept(
            incoming,
            cancellationToken);
    }
}
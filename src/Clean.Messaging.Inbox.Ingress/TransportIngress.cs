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
    TMapper mapper,
    InboxMessageFactory factory,
    IInbox inbox)
    : ITransportIngress
    where TPayload : notnull
    where TMessage : notnull
    where TMapper :
    class,
    ITransportMapper<TMessage>
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
            var message = mapper.Map(
                transport);

            incoming = factory.Create(
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
                $"Transport message '{transport.Contract}' could not be mapped.",
                exception);
        }

        await inbox.Accept(
            incoming,
            cancellationToken);
    }
}
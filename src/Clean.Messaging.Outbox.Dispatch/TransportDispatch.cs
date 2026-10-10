using Clean.Messaging.Outbox.Transport;
using Clean.Messaging.Serialization;
using Clean.Messaging.Transport;

namespace Clean.Messaging.Outbox.Dispatch;

internal interface ITransportDispatch
{
    DispatchMap Map { get; }

    TransportMessage Create(
        OutboxEnvelope envelope);
}

internal sealed class TransportDispatch<
    TMessage,
    TPayload,
    TMapper>(
    IMessageSerializer serializer,
    TMapper mapper,
    TransportMessageFactory factory)
    : ITransportDispatch
    where TMessage : notnull
    where TPayload : notnull
    where TMapper :
        class,
        ITransportMapper<TMessage, TPayload>
{
    public DispatchMap Map { get; } =
        new(
            typeof(TMessage),
            typeof(TPayload));

    public TransportMessage Create(
        OutboxEnvelope envelope)
    {
        try
        {
            var source =
                Deserialize(
                    envelope);

            var payload =
                mapper.Map(
                    source);

            return factory.Create(
                envelope,
                payload);
        }
        catch (OutboxDispatchException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new OutboxDispatchException(
                $"Outbox message '{envelope.Message.Type}' could not be mapped " +
                $"from '{typeof(TMessage)}' to '{typeof(TPayload)}'.",
                exception);
        }
    }

    private TMessage Deserialize(
        OutboxEnvelope envelope)
    {
        var value =
            serializer.Deserialize(
                envelope.Message);

        if (value is TMessage typed)
        {
            return typed;
        }

        throw new OutboxDispatchException(
            $"Outbox message '{envelope.Message.Type}' deserialized as " +
            $"'{value?.GetType()}' but dispatch expects '{typeof(TMessage)}'.");
    }
}

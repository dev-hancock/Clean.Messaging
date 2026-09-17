using Clean.Messaging.Abstractions;
using Clean.Messaging.Sagas.Definition;
using Clean.Messaging.Sagas.Exceptions;
using Clean.Messaging.Serialization;

namespace Clean.Messaging.Sagas.Runtime;

internal sealed class SagaProcessor(
    SagaRegistry registry,
    SagaRuntime runtime,
    IMessageSerializer serializer,
    IMessageContext context)
{
    public ValueTask Process<TState, TMessage>(
        TMessage message,
        CancellationToken cancellationToken)
        where TState : class
        where TMessage : notnull
    {
        var messageId = context.MessageId
                        ?? throw new InvalidOperationException(
                            "Saga message processing requires a durable message context.");

        var definition = registry.Get<TState>();
        var mapping = definition.GetMessage<TMessage>();

        return mapping.Handle(
            runtime,
            definition,
            message,
            new(
                messageId,
                context.CorrelationId,
                context.CausationId),
            cancellationToken);
    }

    public async ValueTask Process(
        SagaId sagaId,
        MessageData data,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        var saga = await runtime.Find(
            sagaId,
            cancellationToken);

        if (saga is null)
        {
            throw new SagaNotFoundException(
                sagaId);
        }

        if (saga.IsTerminal)
        {
            return;
        }

        var definition = registry.Get(
            saga.Type);

        var message = serializer.Deserialize(
            data);

        await definition.Invoke(
            runtime,
            saga,
            message,
            trigger,
            cancellationToken);
    }
}
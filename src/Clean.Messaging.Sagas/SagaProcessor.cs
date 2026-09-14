using Clean.Messaging.Abstractions;
using Clean.Messaging.Serialization;

namespace Clean.Messaging.Sagas;

internal sealed class SagaProcessor(
    SagaRegistry registry,
    SagaEngine engine,
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
        var registration = definition.GetMessage<TMessage>();

        return registration.Execute(
            engine,
            definition,
            message,
            new(
                messageId,
                context.CorrelationId,
                context.CausationId),
            cancellationToken);
    }

    public async ValueTask ProcessTimer(
        SagaId sagaId,
        MessageData data,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        var saga = await engine.Find(
            sagaId,
            cancellationToken);

        if (saga is null)
        {
            throw new SagaTimerNotFoundException(
                sagaId);
        }

        if (saga.IsTerminal)
        {
            return;
        }

        var definition = registry.Get(
            saga.Type);

        var message = serializer.Deserialize(data);

        await definition.InvokeTimer(
            engine,
            saga,
            message,
            trigger,
            cancellationToken);
    }
}

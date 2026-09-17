using Clean.Messaging.Abstractions;

namespace Clean.Messaging.Sagas.Runtime;

internal sealed class SagaConsumer<TState, TMessage>(
    SagaProcessor processor)
    : IMessageConsumer<TMessage>
    where TState : class
    where TMessage : notnull
{
    public ValueTask Handle(
        TMessage message,
        CancellationToken cancellationToken)
    {
        return processor.Process<TState, TMessage>(
            message,
            cancellationToken);
    }
}
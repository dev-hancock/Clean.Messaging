namespace Clean.Messaging.Sagas;

internal interface ISagaTimerRegistration<TState>
    where TState : class
{
    Type MessageType { get; }

    ValueTask Invoke(
        SagaExecution execution,
        SagaDefinition<TState> definition,
        SagaEntry saga,
        object message,
        SagaTrigger trigger,
        CancellationToken cancellationToken);
}

internal sealed class SagaTimerRegistration<
    TState,
    TMessage,
    THandler>
    : ISagaTimerRegistration<TState>
    where TState : class
    where TMessage : notnull
    where THandler : class, ISagaHandler<TState, TMessage>
{
    public Type MessageType => typeof(TMessage);

    public ValueTask Invoke(
        SagaExecution execution,
        SagaDefinition<TState> definition,
        SagaEntry saga,
        object message,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        if (message is not TMessage typed)
        {
            throw new SagaSerializationException(
                $"Saga '{definition.Type}' timer expects '{typeof(TMessage)}' but received '{message.GetType()}'.");
        }

        return execution.Invoke<
            TState,
            TMessage,
            THandler>(
            definition,
            saga,
            typed,
            trigger,
            cancellationToken);
    }
}
namespace Clean.Messaging.Sagas;

internal interface ISagaDefinition
{
    string Type { get; }

    Type StateType { get; }

    IEnumerable<Type> MessageTypes { get; }

    IEnumerable<Type> TimerMessageTypes { get; }

    ValueTask InvokeTimer(
        SagaExecution execution,
        SagaEntry saga,
        object message,
        SagaTrigger trigger,
        CancellationToken cancellationToken);
}

internal sealed class SagaDefinition<TState>(
    string type,
    IReadOnlyDictionary<Type, ISagaMessageRegistration<TState>> messages,
    IReadOnlyDictionary<Type, ISagaTimerRegistration<TState>> timers)
    : ISagaDefinition
    where TState : class
{
    public string Type { get; } = type;

    public Type StateType => typeof(TState);

    public IEnumerable<Type> MessageTypes => messages.Keys;

    public IEnumerable<Type> TimerMessageTypes => timers.Keys;

    public ValueTask InvokeTimer(
        SagaExecution execution,
        SagaEntry saga,
        object message,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        var messageType = message.GetType();

        if (!timers.TryGetValue(messageType, out var registration))
        {
            throw new SagaSerializationException(
                $"Saga '{Type}' has no timer handler for message '{messageType}'.");
        }

        return registration.Invoke(
            execution,
            this,
            saga,
            message,
            trigger,
            cancellationToken);
    }

    public ISagaMessageRegistration<TState, TMessage> GetMessage<TMessage>()
        where TMessage : notnull
    {
        if (messages.TryGetValue(typeof(TMessage), out var registration) &&
            registration is ISagaMessageRegistration<TState, TMessage> typed)
        {
            return typed;
        }

        throw new InvalidOperationException(
            $"Saga '{Type}' has no registration for message '{typeof(TMessage)}'.");
    }
}
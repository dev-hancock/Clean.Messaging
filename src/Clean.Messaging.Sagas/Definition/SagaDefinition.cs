using Clean.Messaging.Sagas.Exceptions;
using Clean.Messaging.Sagas.Persistence;
using Clean.Messaging.Sagas.Runtime;

namespace Clean.Messaging.Sagas.Definition;

internal interface ISagaDefinition
{
    string Type { get; }

    Type StateType { get; }

    IEnumerable<Type> MessageTypes { get; }

    ValueTask Invoke(
        SagaRuntime runtime,
        SagaEntry saga,
        object message,
        SagaTrigger trigger,
        CancellationToken cancellationToken);
}

internal sealed class SagaDefinition<TState>(
    string type,
    IReadOnlyDictionary<Type, ISagaMessageDefinition<TState>> messages)
    : ISagaDefinition
    where TState : class
{
    public string Type { get; } = type;

    public Type StateType => typeof(TState);

    public IEnumerable<Type> MessageTypes => messages.Keys;

    public ValueTask Invoke(
        SagaRuntime runtime,
        SagaEntry saga,
        object message,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        var messageType = message.GetType();

        if (!messages.TryGetValue(
                messageType,
                out var sagaMessage))
        {
            throw new SagaSerializationException(
                $"Saga '{Type}' does not handle message '{messageType}'.");
        }

        return sagaMessage.Handle(
            runtime,
            this,
            saga,
            message,
            trigger,
            cancellationToken);
    }

    public ISagaMessageDefinition<TState> GetMessage<TMessage>()
        where TMessage : notnull
    {
        if (messages.TryGetValue(
                typeof(TMessage),
                out var message))
        {
            return message;
        }

        throw new InvalidOperationException(
            $"Saga '{Type}' does not handle '{typeof(TMessage)}'.");
    }
}
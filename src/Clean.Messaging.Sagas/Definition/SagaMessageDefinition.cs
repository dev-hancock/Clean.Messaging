using Clean.Messaging.Sagas.Exceptions;
using Clean.Messaging.Sagas.Persistence;
using Clean.Messaging.Sagas.Runtime;

namespace Clean.Messaging.Sagas.Definition;

internal interface ISagaMessageDefinition<TState>
    where TState : class
{
    Type MessageType { get; }

    ValueTask Handle(
        SagaRuntime runtime,
        SagaDefinition<TState> definition,
        object message,
        SagaTrigger trigger,
        CancellationToken cancellationToken);

    ValueTask Handle(
        SagaRuntime runtime,
        SagaDefinition<TState> definition,
        SagaEntry saga,
        object message,
        SagaTrigger trigger,
        CancellationToken cancellationToken);
}

internal sealed class SagaMessageDefinition<
    TState,
    TMessage,
    THandler>(
    Func<TMessage, SagaKey>? correlate = null,
    Func<TMessage, TState>? create = null)
    : ISagaMessageDefinition<TState>
    where TState : class
    where TMessage : notnull
    where THandler : class, ISagaHandler<TState, TMessage>
{
    public Type MessageType => typeof(TMessage);

    public async ValueTask Handle(
        SagaRuntime runtime,
        SagaDefinition<TState> definition,
        object message,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        var typed = GetMessage(
            definition,
            message);

        if (correlate is null)
        {
            throw new InvalidOperationException(
                $"Saga '{definition.Type}' message '{typeof(TMessage)}' requires a saga instance.");
        }

        var key = correlate(typed);

        var saga = await runtime.Find(
            definition.Type,
            key,
            cancellationToken);

        if (saga is null)
        {
            if (create is null)
            {
                throw new SagaNotFoundException(
                    definition.Type,
                    key);
            }

            saga = runtime.Create(
                definition,
                key,
                create(typed),
                trigger);
        }
        else if (create is not null)
        {
            if (saga.StartedByMessageId ==
                trigger.MessageId)
            {
                return;
            }

            if (saga.IsTerminal)
            {
                throw new SagaClosedException(
                    definition.Type,
                    key);
            }

            throw new SagaConflictException(
                definition.Type,
                key);
        }
        else if (saga.IsTerminal)
        {
            return;
        }

        await Invoke(
            runtime,
            definition,
            saga,
            typed,
            trigger,
            cancellationToken);
    }

    public ValueTask Handle(
        SagaRuntime runtime,
        SagaDefinition<TState> definition,
        SagaEntry saga,
        object message,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        if (saga.IsTerminal)
        {
            return ValueTask.CompletedTask;
        }

        var typed = GetMessage(
            definition,
            message);

        return Invoke(
            runtime,
            definition,
            saga,
            typed,
            trigger,
            cancellationToken);
    }

    private static TMessage GetMessage(
        SagaDefinition<TState> definition,
        object message)
    {
        if (message is TMessage typed)
        {
            return typed;
        }

        throw new SagaSerializationException(
            $"Saga '{definition.Type}' expects '{typeof(TMessage)}' but received '{message.GetType()}'.");
    }

    private static ValueTask Invoke(
        SagaRuntime runtime,
        SagaDefinition<TState> definition,
        SagaEntry saga,
        TMessage message,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        return runtime.Invoke<
            TState,
            TMessage,
            THandler>(
            definition,
            saga,
            message,
            trigger,
            cancellationToken);
    }
}
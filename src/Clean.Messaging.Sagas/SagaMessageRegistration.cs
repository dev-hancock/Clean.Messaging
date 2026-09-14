namespace Clean.Messaging.Sagas;

internal interface ISagaMessageRegistration<TState>
    where TState : class
{
    Type MessageType { get; }
}

internal interface ISagaMessageRegistration<TState, in TMessage>
    : ISagaMessageRegistration<TState>
    where TState : class
    where TMessage : notnull
{
    ValueTask Execute(
        SagaEngine engine,
        SagaDefinition<TState> definition,
        TMessage message,
        SagaTrigger trigger,
        CancellationToken cancellationToken);
}

internal abstract class SagaMessageRegistration<TState, TMessage>(
    Func<TMessage, SagaKey> correlate)
    : ISagaMessageRegistration<TState, TMessage>
    where TState : class
    where TMessage : notnull
{
    public Type MessageType => typeof(TMessage);

    public abstract ValueTask Execute(
        SagaEngine engine,
        SagaDefinition<TState> definition,
        TMessage message,
        SagaTrigger trigger,
        CancellationToken cancellationToken);

    protected SagaKey Correlate(
        TMessage message)
    {
        return correlate(message);
    }
}

internal sealed class SagaHandleRegistration<
    TState,
    TMessage,
    THandler>(
    Func<TMessage, SagaKey> correlate)
    : SagaMessageRegistration<TState, TMessage>(correlate)
    where TState : class
    where TMessage : notnull
    where THandler : class, ISagaHandler<TState, TMessage>
{
    public override async ValueTask Execute(
        SagaEngine engine,
        SagaDefinition<TState> definition,
        TMessage message,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        var key = Correlate(message);

        var saga = await engine.Find(
            definition.Type,
            key,
            cancellationToken);

        if (saga is null)
        {
            throw new SagaNotFoundException(
                definition.Type,
                key);
        }

        if (saga.IsTerminal)
        {
            return;
        }

        await engine.Invoke<
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

internal sealed class SagaStartRegistration<
    TState,
    TMessage,
    THandler>(
    Func<TMessage, SagaKey> correlate,
    Func<TMessage, TState> create)
    : SagaMessageRegistration<TState, TMessage>(correlate)
    where TState : class
    where TMessage : notnull
    where THandler : class, ISagaHandler<TState, TMessage>
{
    public override async ValueTask Execute(
        SagaEngine engine,
        SagaDefinition<TState> definition,
        TMessage message,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        var key = Correlate(message);

        var saga = await engine.Find(
            definition.Type,
            key,
            cancellationToken);

        if (saga is not null)
        {
            if (saga.StartedByMessageId == trigger.MessageId)
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

        saga = engine.Create(
            definition,
            key,
            create(message),
            trigger);

        await engine.Invoke<
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

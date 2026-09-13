namespace Clean.Messaging.Sagas;

public interface ISagaHandler<in TState, in TMessage>
    where TState : class
    where TMessage : notnull
{
    ValueTask<SagaResult> Handle(
        TState state,
        TMessage message,
        CancellationToken cancellationToken);
}
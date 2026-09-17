using Clean.Messaging.Sagas.Definition;
using Clean.Messaging.Sagas.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Sagas.Runtime;

internal sealed class SagaAttempt(
    IServiceProvider services,
    SagaStateSerializer serializer)
{
    public async ValueTask<SagaCommit> Run<
        TState,
        TMessage,
        THandler>(
        SagaDefinition<TState> definition,
        SagaEntry saga,
        TMessage message,
        CancellationToken cancellationToken)
        where TState : class
        where TMessage : notnull
        where THandler : class, ISagaHandler<TState, TMessage>
    {
        var state = serializer.Deserialize<TState>(saga);

        var handler = services
            .GetRequiredService<THandler>();

        var result = await handler.Handle(
            state,
            message,
            cancellationToken);

        ArgumentNullException.ThrowIfNull(result);

        return new(
            saga.Version,
            serializer.Serialize(state),
            result);
    }
}
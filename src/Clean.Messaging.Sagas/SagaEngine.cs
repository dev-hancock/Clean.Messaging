using Clean.Messaging.Scheduling;

namespace Clean.Messaging.Sagas;

internal sealed class SagaEngine(
    ISagaStore store,
    IScheduledMessageWriter scheduler,
    SagaAttempt attempt,
    SagaEffectWriter effects,
    SagaStateSerializer serializer,
    TimeProvider time)
{
    public ValueTask<SagaEntry?> Find(
        string sagaType,
        SagaKey key,
        CancellationToken cancellationToken)
    {
        return store.Find(
            sagaType,
            key,
            cancellationToken);
    }

    public ValueTask<SagaEntry?> Find(
        SagaId id,
        CancellationToken cancellationToken)
    {
        return store.Find(
            id,
            cancellationToken);
    }

    public SagaEntry Create<TState>(
        SagaDefinition<TState> definition,
        SagaKey key,
        TState state,
        SagaTrigger trigger)
        where TState : class
    {
        ArgumentNullException.ThrowIfNull(state);

        var now = time.GetUtcNow().UtcDateTime;

        var saga = new SagaEntry
        {
            Id = SagaId.New(),
            Type = definition.Type,
            Key = key,
            State = serializer.Serialize(state),
            Version = 0,
            StartedByMessageId = trigger.MessageId,
            CreatedAt = now,
            UpdatedAt = now
        };

        store.Add(saga);

        return saga;
    }

    public async ValueTask Invoke<
        TState,
        TMessage,
        THandler>(
        SagaDefinition<TState> definition,
        SagaEntry saga,
        TMessage message,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
        where TState : class
        where TMessage : notnull
        where THandler : class, ISagaHandler<TState, TMessage>
    {
        var commit = await attempt.Run<
            TState,
            TMessage,
            THandler>(
            definition,
            saga,
            message,
            cancellationToken);

        var now = time.GetUtcNow().UtcDateTime;

        saga.ApplyState(
            commit.State,
            commit.ExpectedVersion,
            now);

        commit.Result.Apply(
            saga,
            now);

        foreach (var effect in commit.Result.Effects)
        {
            await effect.Apply(
                effects,
                saga,
                trigger,
                cancellationToken);
        }

        if (saga.IsTerminal)
        {
            var except = trigger.TimerId is { } timerId
                ? new ScheduleId(timerId)
                : (ScheduleId?)null;

            await scheduler.CancelGroup(
                new ScheduleGroupId(saga.Id.Value),
                SagaScheduledMessageDelivery.Target,
                except,
                cancellationToken);
        }
    }
}

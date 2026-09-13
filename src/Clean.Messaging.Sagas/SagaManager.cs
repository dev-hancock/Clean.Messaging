using Clean.Messaging.Scheduling;

namespace Clean.Messaging.Sagas;

internal sealed class SagaManager(
    ISagaStore sagas,
    IScheduledMessageReader scheduler)
    : ISagaManager
{
    public async ValueTask<Saga?> Get(
        SagaId id,
        CancellationToken cancellationToken = default)
    {
        var saga = await sagas.Get(
            id,
            cancellationToken);

        return saga is null
            ? null
            : Map(saga);
    }

    public async ValueTask<IReadOnlyList<Saga>> GetActive(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        var entries = await sagas.GetActive(
            limit,
            cancellationToken);

        return entries
            .Select(Map)
            .ToArray();
    }

    public async ValueTask<IReadOnlyList<Saga>> GetFailed(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        var entries = await sagas.GetFailed(
            limit,
            cancellationToken);

        return entries
            .Select(Map)
            .ToArray();
    }

    public async ValueTask<IReadOnlyList<SagaTimer>> GetTimers(
        SagaId sagaId,
        CancellationToken cancellationToken = default)
    {
        var saga = await sagas.Get(
            sagaId,
            cancellationToken);

        if (saga is null)
        {
            return [];
        }

        var entries = await scheduler.GetGroup(
            new ScheduleGroupId(sagaId.Value),
            SagaScheduledMessageDelivery.Target,
            cancellationToken);

        return entries
            .Select(timer => Map(
                saga,
                timer))
            .ToArray();
    }

    private static Saga Map(
        SagaEntry saga)
    {
        return new(
            saga.Id,
            saga.Type,
            saga.Key,
            saga.Version,
            saga.CreatedAt,
            saga.UpdatedAt,
            saga.StatePurgedAt,
            saga.CompletedAt,
            saga.FailedAt,
            saga.FailureCode,
            saga.FailureMessage);
    }

    private static SagaTimer Map(
        SagaEntry saga,
        ScheduledMessageStatus timer)
    {
        return new(
            timer.ScheduleId.Value,
            saga.Id,
            saga.Type,
            timer.MessageData.Type,
            timer.DueAt,
            timer.Attempts,
            timer.NextAttemptAt,
            timer.DispatchedAt,
            timer.CancelledAt,
            timer.FailedAt,
            timer.FailureCode,
            timer.FailureMessage);
    }
}

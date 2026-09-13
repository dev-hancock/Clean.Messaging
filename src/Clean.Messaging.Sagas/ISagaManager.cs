namespace Clean.Messaging.Sagas;

public interface ISagaManager
{
    ValueTask<Saga?> Get(
        SagaId id,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<Saga>> GetActive(
        int limit = 100,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<Saga>> GetFailed(
        int limit = 100,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<SagaTimer>> GetTimers(
        SagaId sagaId,
        CancellationToken cancellationToken = default);
}

namespace Clean.Messaging.Sagas;

internal interface ISagaStore
{
    ValueTask<SagaEntry?> Find(
        string sagaType,
        SagaKey key,
        CancellationToken cancellationToken);

    ValueTask<SagaEntry?> Find(
        SagaId id,
        CancellationToken cancellationToken);

    ValueTask<SagaEntry?> Get(
        SagaId id,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<SagaEntry>> GetActive(
        int limit,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<SagaEntry>> GetFailed(
        int limit,
        CancellationToken cancellationToken);

    void Add(SagaEntry saga);

    ValueTask<int> PurgeState(
        DateTime before,
        DateTime purgedAt,
        int limit,
        CancellationToken cancellationToken);
}

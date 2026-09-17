using Clean.Messaging.Sagas.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clean.Messaging.Sagas.EntityFrameworkCore;

internal sealed class SagaStore<TDbContext>(
    TDbContext db)
    : ISagaStore
    where TDbContext : DbContext
{
    public ValueTask<SagaEntry?> Find(
        string sagaType,
        SagaKey key,
        CancellationToken cancellationToken)
    {
        return new(
            db.Set<SagaEntry>()
                .SingleOrDefaultAsync(
                    saga =>
                        saga.Type == sagaType &&
                        saga.Key == key,
                    cancellationToken));
    }

    public ValueTask<SagaEntry?> Find(
        SagaId id,
        CancellationToken cancellationToken)
    {
        return new(
            db.Set<SagaEntry>()
                .SingleOrDefaultAsync(
                    saga => saga.Id == id,
                    cancellationToken));
    }

    public ValueTask<SagaEntry?> Get(
        SagaId id,
        CancellationToken cancellationToken)
    {
        return new(
            db.Set<SagaEntry>()
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    saga => saga.Id == id,
                    cancellationToken));
    }

    public async ValueTask<IReadOnlyList<SagaEntry>> GetActive(
        int limit,
        CancellationToken cancellationToken)
    {
        return await db.Set<SagaEntry>()
            .AsNoTracking()
            .Where(saga =>
                saga.CompletedAt == null &&
                saga.FailedAt == null)
            .OrderBy(saga => saga.CreatedAt)
            .Take(limit)
            .ToArrayAsync(cancellationToken);
    }

    public async ValueTask<IReadOnlyList<SagaEntry>> GetFailed(
        int limit,
        CancellationToken cancellationToken)
    {
        return await db.Set<SagaEntry>()
            .AsNoTracking()
            .Where(saga => saga.FailedAt != null)
            .OrderByDescending(saga => saga.FailedAt)
            .Take(limit)
            .ToArrayAsync(cancellationToken);
    }

    public void Add(
        SagaEntry saga)
    {
        db.Set<SagaEntry>()
            .Add(saga);
    }

    public async ValueTask<int> PurgeState(
        DateTime before,
        DateTime purgedAt,
        int limit,
        CancellationToken cancellationToken)
    {
        return await db.Set<SagaEntry>()
            .Where(saga =>
                saga.StatePurgedAt == null &&
                (
                    (saga.CompletedAt != null && saga.CompletedAt < before) ||
                    (saga.FailedAt != null && saga.FailedAt < before)
                ))
            .OrderBy(saga => saga.UpdatedAt)
            .Take(limit)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        saga => saga.State,
                        SagaEntry.TombstoneState)
                    .SetProperty(
                        saga => saga.StatePurgedAt,
                        purgedAt),
                cancellationToken);
    }
}

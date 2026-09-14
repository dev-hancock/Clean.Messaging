using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Sagas.EntityFrameworkCore;

internal sealed class SagaConflictResolver<TDbContext>(
    IServiceScopeFactory scopes)
    : ISagaConflictResolver
    where TDbContext : DbContext
{
    public Exception? Resolve(
        Exception exception)
    {
        if (exception is not DbUpdateException update ||
            !TryGet(update, out var start))
        {
            return null;
        }

        var persisted = Find(start);

        return CreateException(
            persisted,
            start);
    }

    public async ValueTask<Exception?> ResolveAsync(
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateException update ||
            !TryGet(update, out var start))
        {
            return null;
        }

        var persisted = await Find(
            start,
            cancellationToken);

        return CreateException(
            persisted,
            start);
    }

    private SagaEntry? Find(
        SagaIdentity start)
    {
        try
        {
            using var scope = scopes.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<TDbContext>();

            return db.Set<SagaEntry>()
                .AsNoTracking()
                .SingleOrDefault(saga =>
                    saga.Type == start.Type &&
                    saga.Key == start.Key);
        }
        catch
        {
            return null;
        }
    }

    private async ValueTask<SagaEntry?> Find(
        SagaIdentity start,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();

            var db = scope.ServiceProvider
                .GetRequiredService<TDbContext>();

            return await db.Set<SagaEntry>()
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    saga =>
                        saga.Type == start.Type &&
                        saga.Key == start.Key,
                    cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private static Exception? CreateException(
        SagaEntry? persisted,
        SagaIdentity start)
    {
        return persisted switch
        {
            { StartedByMessageId: var messageId } when messageId == start.MessageId => new SagaRaceException(
                start.Type,
                start.Key,
                start.MessageId),
            null => null,
            { IsTerminal: true } => new SagaClosedException(
                start.Type,
                start.Key),
            _ => new SagaConflictException(
                start.Type,
                start.Key)
        };
    }

    private static bool TryGet(
        DbUpdateException exception,
        out SagaIdentity start)
    {
        var starts = exception.Entries
            .Where(entry =>
                entry is
                {
                    State: EntityState.Added,
                    Entity: SagaEntry
                })
            .Select(entry => (SagaEntry)entry.Entity)
            .Select(saga => new SagaIdentity(
                saga.Type,
                saga.Key,
                saga.StartedByMessageId))
            .Distinct()
            .ToArray();

        if (starts.Length == 1)
        {
            start = starts[0];

            return true;
        }

        start = default;

        return false;
    }

    private readonly record struct SagaIdentity(
        string Type,
        SagaKey Key,
        Guid MessageId);
}

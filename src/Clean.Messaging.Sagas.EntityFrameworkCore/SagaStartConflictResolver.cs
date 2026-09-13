using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Sagas.EntityFrameworkCore;

internal sealed class SagaStartConflictResolver<TDbContext>(
    IServiceScopeFactory scopes)
    : ISagaStartConflictResolver
    where TDbContext : DbContext
{
    public Exception? Resolve(
        Exception exception)
    {
        if (exception is not DbUpdateException update ||
            !TryGetStart(update, out var start))
        {
            return null;
        }

        var persisted = FindPersisted(start);

        return CreateException(
            persisted,
            start);
    }

    public async ValueTask<Exception?> ResolveAsync(
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateException update ||
            !TryGetStart(update, out var start))
        {
            return null;
        }

        var persisted = await FindPersisted(
            start,
            cancellationToken);

        return CreateException(
            persisted,
            start);
    }

    private SagaEntry? FindPersisted(
        SagaStartIdentity start)
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

    private async ValueTask<SagaEntry?> FindPersisted(
        SagaStartIdentity start,
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
        SagaStartIdentity start)
    {
        return persisted switch
        {
            null => null,
            { IsTerminal: true } => new SagaStartClosedException(
                start.Type,
                start.Key),
            _ => new SagaStartConflictException(
                start.Type,
                start.Key)
        };
    }

    private static bool TryGetStart(
        DbUpdateException exception,
        out SagaStartIdentity start)
    {
        var starts = exception.Entries
            .Where(entry =>
                entry is
                {
                    State: EntityState.Added,
                    Entity: SagaEntry
                })
            .Select(entry => (SagaEntry)entry.Entity)
            .Select(saga => new SagaStartIdentity(
                saga.Type,
                saga.Key))
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

    private readonly record struct SagaStartIdentity(
        string Type,
        SagaKey Key);
}

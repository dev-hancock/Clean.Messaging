using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Clean.Messaging.Sagas.EntityFrameworkCore;

internal sealed class SagaSaveInterceptor(
    ISagaStartConflictResolver conflicts)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is { } db)
        {
            EnsureNoAmbientTransaction(db);
        }

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(
            SavingChanges(
                eventData,
                result));
    }

    public override void SaveChangesFailed(
        DbContextErrorEventData eventData)
    {
        if (eventData.Context is null)
        {
            return;
        }

        if (conflicts.Resolve(
                eventData.Exception) is { } translated)
        {
            throw translated;
        }
    }

    public override async Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return;
        }

        if (await conflicts.ResolveAsync(
                eventData.Exception,
                cancellationToken) is { } translated)
        {
            throw translated;
        }
    }

    private static void EnsureNoAmbientTransaction(
        DbContext db)
    {
        if (Transaction.Current is null)
        {
            return;
        }

        var changed = db.ChangeTracker
            .Entries<SagaEntry>()
            .Any(entry => entry.State is
                EntityState.Added or
                EntityState.Modified or
                EntityState.Deleted);

        if (!changed)
        {
            return;
        }

        throw new InvalidOperationException(
            "Saga persistence does not support ambient transactions. Use an EF transaction.");
    }
}

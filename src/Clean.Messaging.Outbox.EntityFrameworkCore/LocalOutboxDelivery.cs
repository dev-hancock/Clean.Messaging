using Clean.Messaging.Outbox.Delivery;
using Clean.Messaging.Outbox.Persistence;
using Clean.Messaging.Outbox.Transport;
using Clean.Messaging.Persistence;
using Clean.Messaging.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Outbox.EntityFrameworkCore;

internal sealed class LocalOutboxDelivery<TDbContext>(
    TDbContext strategyContext,
    IServiceScopeFactory scopes,
    TimeProvider time)
    : IOutboxDelivery
    where TDbContext : DbContext
{
    public string Transport =>
        LocalTransport.Name;

    public async ValueTask<bool> Dispatch(
        OwnedEntry<OutboxEntry> owned,
        CancellationToken cancellationToken)
    {
        var strategy =
            strategyContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var scope =
                scopes.CreateAsyncScope();

            var services =
                scope.ServiceProvider;

            var db = services
                .GetRequiredService<TDbContext>();

            if (await IsCompleted(
                    db,
                    owned.Entry,
                    cancellationToken))
            {
                return true;
            }

            var store = services
                .GetRequiredService<IOutboxStore>();

            var dispatcher = services
                .GetRequiredService<ConsumerDispatcher>();

            return await Dispatch(
                db,
                store,
                dispatcher,
                owned,
                cancellationToken);
        });
    }

    private async Task<bool> Dispatch(
        TDbContext db,
        IOutboxStore store,
        ConsumerDispatcher dispatcher,
        OwnedEntry<OutboxEntry> owned,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            await dispatcher.Dispatch(
                owned.Entry.TargetId,
                owned.Entry.Message,
                cancellationToken);

            var completed = await store.Complete(
                owned.Entry.Key,
                owned.ClaimId,
                time.GetUtcNow().UtcDateTime,
                cancellationToken);

            if (!completed)
            {
                await transaction.RollbackAsync(
                    CancellationToken.None);

                return false;
            }

            await db.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return true;
        }
        catch
        {
            await Rollback(transaction);

            throw;
        }
    }

    private static Task<bool> IsCompleted(
        TDbContext db,
        OutboxEntry entry,
        CancellationToken cancellationToken)
    {
        return db.Set<OutboxEntry>()
            .AsNoTracking()
            .AnyAsync(
                candidate =>
                    candidate.MessageId == entry.MessageId &&
                    candidate.TargetId == entry.TargetId &&
                    candidate.ProcessedAt != null,
                cancellationToken);
    }

    private static async Task Rollback(
        IDbContextTransaction transaction)
    {
        try
        {
            await transaction.RollbackAsync(
                CancellationToken.None);
        }
        catch
        {
            // Preserve the original failure,
            // including an uncertain commit.
        }
    }
}
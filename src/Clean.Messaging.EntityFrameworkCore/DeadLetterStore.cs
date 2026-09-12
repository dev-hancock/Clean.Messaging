using Clean.Messaging.Abstractions;
using Clean.Messaging.DeadLetters;
using Clean.Messaging.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Transactions;

namespace Clean.Messaging.EntityFrameworkCore;

internal sealed class DeadLetterStore<
    TDbContext,
    TEntry>(
    TDbContext db,
    TimeProvider time)
    : IDeadLetterStore<TEntry>
    where TDbContext : DbContext
    where TEntry : class, IDurableEntry
{
    public async ValueTask<IReadOnlyList<DeadLetter>> Get(
        int limit,
        CancellationToken cancellationToken)
    {
        return await DeadLetters()
            .AsNoTracking()
            .OrderBy(entry => entry.DeadLetteredAt)
            .Take(limit)
            .Select(entry => new DeadLetter(
                new EntryKey(
                    entry.MessageId,
                    entry.TargetId),
                entry.EventId,
                entry.DeadLetteredAt!.Value,
                entry.Attempts,
                entry.FailureCode,
                entry.FailureMessage))
            .ToArrayAsync(cancellationToken);
    }

    public async ValueTask<bool> Requeue(
        EntryKey key,
        CancellationToken cancellationToken)
    {
        EnsureIndependentTransaction();

        var strategy = db.Database
            .CreateExecutionStrategy();

        return await strategy.ExecuteInTransactionAsync(
            operation: token => RequeueEntry(
                key,
                token),
            verifySucceeded: token => RequeueSucceeded(
                key,
                token),
            cancellationToken);
    }

    public async ValueTask<bool> Discard(
        EntryKey key,
        CancellationToken cancellationToken)
    {
        EnsureIndependentTransaction();

        var discardedAt = time
            .GetUtcNow()
            .UtcDateTime;

        var strategy = db.Database
            .CreateExecutionStrategy();

        return await strategy.ExecuteInTransactionAsync(
            operation: token => DiscardEntry(
                key,
                discardedAt,
                token),
            verifySucceeded: token => DiscardSucceeded(
                key,
                token),
            cancellationToken);
    }

    private async Task<bool> RequeueEntry(
        EntryKey key,
        CancellationToken cancellationToken)
    {
        var affected = await DeadLetters()
            .Where(HasKey(key))
            .ExecuteUpdateAsync(
                SetRequeued(),
                cancellationToken);

        return affected == 1;
    }

    private async Task<bool> DiscardEntry(
        EntryKey key,
        DateTime discardedAt,
        CancellationToken cancellationToken)
    {
        var affected = await DeadLetters()
            .Where(HasKey(key))
            .ExecuteUpdateAsync(
                SetDiscarded(discardedAt),
                cancellationToken);

        return affected == 1;
    }

    private IQueryable<TEntry> DeadLetters()
    {
        return db
            .Set<TEntry>()
            .Where(entry =>
                entry.DeadLetteredAt != null &&
                entry.DiscardedAt == null);
    }

    private Task<bool> RequeueSucceeded(
        EntryKey key,
        CancellationToken cancellationToken)
    {
        return db
            .Set<TEntry>()
            .AsNoTracking()
            .Where(HasKey(key))
            .AnyAsync(
                entry =>
                    entry.ProcessedAt == null &&
                    entry.DeadLetteredAt == null &&
                    entry.DiscardedAt == null &&
                    entry.ClaimId == null &&
                    entry.ClaimedUntil == null &&
                    entry.Attempts == 0,
                cancellationToken);
    }

    private Task<bool> DiscardSucceeded(
        EntryKey key,
        CancellationToken cancellationToken)
    {
        return db
            .Set<TEntry>()
            .AsNoTracking()
            .Where(HasKey(key))
            .AnyAsync(
                entry =>
                    entry.DiscardedAt != null,
                cancellationToken);
    }

    private static Expression<Func<TEntry, bool>> HasKey(
        EntryKey key)
    {
        return entry =>
            entry.MessageId == key.MessageId &&
            entry.TargetId == key.TargetId;
    }

    private static Action<UpdateSettersBuilder<TEntry>> SetRequeued()
    {
        return setters =>
        {
            setters.SetProperty(
                entry => entry.DeadLetteredAt,
                (DateTime?)null);

            setters.SetProperty(
                entry => entry.DiscardedAt,
                (DateTime?)null);

            setters.SetProperty(
                entry => entry.NextAttemptAt,
                (DateTime?)null);

            setters.SetProperty(
                entry => entry.ClaimId,
                (string?)null);

            setters.SetProperty(
                entry => entry.ClaimedUntil,
                (DateTime?)null);

            setters.SetProperty(
                entry => entry.Attempts,
                0);
        };
    }

    private static Action<UpdateSettersBuilder<TEntry>> SetDiscarded(
        DateTime discardedAt)
    {
        return setters =>
        {
            setters.SetProperty(
                entry => entry.DiscardedAt,
                discardedAt);

            setters.SetProperty(
                entry => entry.NextAttemptAt,
                (DateTime?)null);

            setters.SetProperty(
                entry => entry.ClaimId,
                (string?)null);

            setters.SetProperty(
                entry => entry.ClaimedUntil,
                (DateTime?)null);
        };
    }

    private static void EnsureIndependentTransaction()
    {
        if (Transaction.Current is not null)
        {
            throw new InvalidOperationException(
                "Dead-letter operations cannot run inside an ambient transaction.");
        }
    }
}

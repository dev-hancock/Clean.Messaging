using Clean.Messaging.Abstractions;
using Clean.Messaging.Failures;
using Clean.Messaging.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace Clean.Messaging.EntityFrameworkCore;

internal abstract class DurableStore<TDbContext, TEntry>(
    TDbContext db,
    IServiceScopeFactory scopes)
    : IDurableStore<TEntry>
    where TDbContext : DbContext
    where TEntry : class, IDurableEntry
{
    protected TDbContext Db { get; } = db;

    protected AsyncServiceScope CreateScope()
    {
        return scopes.CreateAsyncScope();
    }

    public async ValueTask<IReadOnlyList<OwnedEntry<TEntry>>> Claim(
        DateTime now,
        int limit,
        TimeSpan lease,
        CancellationToken cancellationToken)
    {
        ValidateClaim(
            limit,
            lease);

        var claimId = ClaimId.Create();
        var claimedUntil = now.Add(lease);

        var entries = await ExecuteClaim(
            claimId,
            now,
            claimedUntil,
            limit,
            cancellationToken);

        return entries
            .Select(entry => new OwnedEntry<TEntry>(
                entry,
                claimId))
            .ToArray();
    }

    public async ValueTask<bool> Renew(
        EntryKey key,
        string claimId,
        DateTime now,
        DateTime claimedUntil,
        CancellationToken cancellationToken)
    {
        await using var scope = CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<TDbContext>();

        return await RenewClaim(
            context,
            key,
            claimId,
            now,
            claimedUntil,
            cancellationToken);
    }

    public ValueTask<bool> Complete(
        EntryKey key,
        string claimId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        return CompleteEntry(
            Db,
            key,
            claimId,
            now,
            cancellationToken);
    }

    public async ValueTask<bool> Retry(
        EntryKey key,
        string claimId,
        MessageFailure failure,
        DateTime now,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken)
    {
        await using var scope = CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<TDbContext>();

        return await ScheduleRetry(
            context,
            key,
            claimId,
            failure,
            now,
            nextAttemptAt,
            cancellationToken);
    }

    public async ValueTask<bool> DeadLetter(
        EntryKey key,
        string claimId,
        MessageFailure failure,
        DateTime now,
        CancellationToken cancellationToken)
    {
        await using var scope = CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<TDbContext>();

        return await ExecuteDeadLetter(
            context,
            key,
            claimId,
            failure,
            now,
            cancellationToken);
    }

    public async ValueTask<int> Cleanup(
        DateTime before,
        int limit,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        await using var scope = CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<TDbContext>();

        return await context
            .Set<TEntry>()
            .Where(entry =>
                (entry.ProcessedAt != null && entry.ProcessedAt < before) ||
                (entry.DeadLetteredAt != null && entry.DeadLetteredAt < before) ||
                (entry.DiscardedAt != null && entry.DiscardedAt < before))
            .OrderBy(entry => entry.CreatedAt)
            .Take(limit)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private Task<TEntry[]> ExecuteClaim(
        string claimId,
        DateTime now,
        DateTime claimedUntil,
        int limit,
        CancellationToken cancellationToken)
    {
        var strategy = Db.Database
            .CreateExecutionStrategy();

        return strategy.ExecuteInTransactionAsync(
            operation: token => ClaimEntries(
                claimId,
                now,
                claimedUntil,
                limit,
                token),
            verifySucceeded: token => ClaimExists(
                Db,
                claimId,
                token),
            cancellationToken);
    }

    private async Task<TEntry[]> ClaimEntries(
        string claimId,
        DateTime now,
        DateTime claimedUntil,
        int limit,
        CancellationToken cancellationToken)
    {
        var affected = await Available(
                Db,
                now)
            .OrderBy(entry => entry.CreatedAt)
            .ThenBy(entry => entry.MessageId)
            .ThenBy(entry => entry.TargetId)
            .Take(limit)
            .ExecuteUpdateAsync(
                SetClaimed(
                    claimId,
                    claimedUntil),
                cancellationToken);

        if (affected == 0)
        {
            return [];
        }

        var entries = await Claimed(
                Db,
                claimId)
            .AsNoTracking()
            .OrderBy(entry => entry.CreatedAt)
            .ThenBy(entry => entry.MessageId)
            .ThenBy(entry => entry.TargetId)
            .ToArrayAsync(cancellationToken);

        if (entries.Length != affected)
        {
            throw new InvalidOperationException(
                $"Claimed {affected} entries but recovered {entries.Length}.");
        }

        return entries;
    }

    private async ValueTask<bool> RenewClaim(
        DbContext context,
        EntryKey key,
        string claimId,
        DateTime now,
        DateTime claimedUntil,
        CancellationToken cancellationToken)
    {
        var affected = await Owned(
                context,
                key,
                claimId,
                now)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    entry => entry.ClaimedUntil,
                    claimedUntil),
                cancellationToken);

        return affected == 1;
    }

    private async ValueTask<bool> CompleteEntry(
        DbContext context,
        EntryKey key,
        string claimId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var affected = await Owned(
                context,
                key,
                claimId,
                now)
            .ExecuteUpdateAsync(
                SetCompleted(now),
                cancellationToken);

        return affected == 1;
    }

    private async ValueTask<bool> ScheduleRetry(
        DbContext context,
        EntryKey key,
        string claimId,
        MessageFailure failure,
        DateTime now,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken)
    {
        var affected = await Owned(
                context,
                key,
                claimId,
                now)
            .ExecuteUpdateAsync(
                SetRetry(
                    failure,
                    nextAttemptAt),
                cancellationToken);

        return affected == 1;
    }

    private Task<bool> ExecuteDeadLetter(
        TDbContext context,
        EntryKey key,
        string claimId,
        MessageFailure failure,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var strategy = context.Database
            .CreateExecutionStrategy();

        return strategy.ExecuteInTransactionAsync(
            operation: token => MarkDeadLettered(
                context,
                key,
                claimId,
                failure,
                now,
                token),
            verifySucceeded: token => IsDeadLettered(
                context,
                key,
                token),
            cancellationToken);
    }

    private async Task<bool> MarkDeadLettered(
        DbContext context,
        EntryKey key,
        string claimId,
        MessageFailure failure,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var affected = await Owned(
                context,
                key,
                claimId,
                now)
            .ExecuteUpdateAsync(
                SetDeadLettered(
                    failure,
                    now),
                cancellationToken);

        return affected == 1;
    }

    private IQueryable<TEntry> Available(
        DbContext context,
        DateTime now)
    {
        return context
            .Set<TEntry>()
            .Where(Unresolved())
            .Where(Due(now))
            .Where(UnclaimedOrExpired(now));
    }

    private IQueryable<TEntry> Claimed(
        DbContext context,
        string claimId)
    {
        return context
            .Set<TEntry>()
            .Where(entry =>
                entry.ClaimId == claimId);
    }

    private IQueryable<TEntry> Owned(
        DbContext context,
        EntryKey key,
        string claimId,
        DateTime now)
    {
        return context
            .Set<TEntry>()
            .Where(HasKey(key))
            .Where(Unresolved())
            .Where(entry =>
                entry.NextAttemptAt == null)
            .Where(ClaimedBy(
                claimId,
                now));
    }

    private Task<bool> ClaimExists(
        DbContext context,
        string claimId,
        CancellationToken cancellationToken)
    {
        return Claimed(
                context,
                claimId)
            .AsNoTracking()
            .AnyAsync(cancellationToken);
    }

    private Task<bool> IsDeadLettered(
        DbContext context,
        EntryKey key,
        CancellationToken cancellationToken)
    {
        return context
            .Set<TEntry>()
            .AsNoTracking()
            .Where(HasKey(key))
            .AnyAsync(
                entry =>
                    entry.DeadLetteredAt != null,
                cancellationToken);
    }

    private static void ValidateClaim(
        int limit,
        TimeSpan lease)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            limit);

        if (lease <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lease),
                lease,
                "Lease duration must be greater than zero.");
        }
    }

    private static Expression<Func<TEntry, bool>> HasKey(
        EntryKey key)
    {
        return entry =>
            entry.MessageId == key.MessageId &&
            entry.TargetId == key.TargetId;
    }

    private static Expression<Func<TEntry, bool>> Unresolved()
    {
        return entry =>
            entry.ProcessedAt == null &&
            entry.DeadLetteredAt == null &&
            entry.DiscardedAt == null;
    }

    private static Expression<Func<TEntry, bool>> Due(
        DateTime now)
    {
        return entry =>
            entry.NextAttemptAt == null ||
            entry.NextAttemptAt <= now;
    }

    private static Expression<Func<TEntry, bool>> UnclaimedOrExpired(
        DateTime now)
    {
        return entry =>
            entry.ClaimId == null ||
            entry.ClaimedUntil == null ||
            entry.ClaimedUntil <= now;
    }

    private static Expression<Func<TEntry, bool>> ClaimedBy(
        string claimId,
        DateTime now)
    {
        return entry =>
            entry.ClaimId == claimId &&
            entry.ClaimedUntil != null &&
            entry.ClaimedUntil > now;
    }

    private static Action<UpdateSettersBuilder<TEntry>> SetClaimed(
        string claimId,
        DateTime claimedUntil)
    {
        return setters =>
        {
            setters.SetProperty(
                entry => entry.ClaimId,
                claimId);

            setters.SetProperty(
                entry => entry.ClaimedUntil,
                claimedUntil);

            setters.SetProperty(
                entry => entry.NextAttemptAt,
                (DateTime?)null);

            setters.SetProperty(
                entry => entry.Attempts,
                entry => entry.Attempts + 1);
        };
    }

    private static Action<UpdateSettersBuilder<TEntry>> SetCompleted(
        DateTime now)
    {
        return setters =>
        {
            ClearClaim(setters);

            setters.SetProperty(
                entry => entry.NextAttemptAt,
                (DateTime?)null);

            setters.SetProperty(
                entry => entry.ProcessedAt,
                now);
        };
    }

    private static Action<UpdateSettersBuilder<TEntry>> SetRetry(
        MessageFailure failure,
        DateTime nextAttemptAt)
    {
        return setters =>
        {
            ClearClaim(setters);
            SetFailure(
                setters,
                failure);

            setters.SetProperty(
                entry => entry.NextAttemptAt,
                nextAttemptAt);
        };
    }

    private static Action<UpdateSettersBuilder<TEntry>> SetDeadLettered(
        MessageFailure failure,
        DateTime now)
    {
        return setters =>
        {
            ClearClaim(setters);
            SetFailure(
                setters,
                failure);

            setters.SetProperty(
                entry => entry.NextAttemptAt,
                (DateTime?)null);

            setters.SetProperty(
                entry => entry.DeadLetteredAt,
                now);
        };
    }

    private static void ClearClaim(
        UpdateSettersBuilder<TEntry> setters)
    {
        setters.SetProperty(
            entry => entry.ClaimId,
            (string?)null);

        setters.SetProperty(
            entry => entry.ClaimedUntil,
            (DateTime?)null);
    }

    private static void SetFailure(
        UpdateSettersBuilder<TEntry> setters,
        MessageFailure failure)
    {
        setters.SetProperty(
            entry => entry.FailureCategory,
            failure.Category);

        setters.SetProperty(
            entry => entry.FailureCode,
            failure.Code);

        setters.SetProperty(
            entry => entry.ExceptionType,
            failure.ExceptionType);

        setters.SetProperty(
            entry => entry.FailureMessage,
            failure.Message);

        setters.SetProperty(
            entry => entry.LastFailedAt,
            failure.FailedAt);

        setters.SetProperty(
            entry => entry.LastFailedAttempt,
            failure.Attempt);
    }
}

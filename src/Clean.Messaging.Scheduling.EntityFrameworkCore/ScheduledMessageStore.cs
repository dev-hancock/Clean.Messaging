using Microsoft.EntityFrameworkCore;

namespace Clean.Messaging.Scheduling.EntityFrameworkCore;

internal sealed class ScheduledMessageStore<TDbContext>(
    TDbContext db)
    : IScheduledMessageStore
    where TDbContext : DbContext
{
    public void Add(
        ScheduledMessageEntry message)
    {
        db.Set<ScheduledMessageEntry>()
            .Add(message);
    }

    public async ValueTask Cancel(
        ScheduleId id,
        ScheduleGroupId? groupId,
        ScheduledMessageTarget? target,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var tracked = db.ChangeTracker
            .Entries<ScheduledMessageEntry>()
            .Select(entry => entry.Entity)
            .SingleOrDefault(message =>
                message.Id == id &&
                Matches(message, groupId, target));

        if (tracked is not null)
        {
            tracked.Cancel(now);
            return;
        }

        var query = db.Set<ScheduledMessageEntry>()
            .Where(message => message.Id == id);

        query = Filter(
            query,
            groupId,
            target);

        var message = await query
            .SingleOrDefaultAsync(cancellationToken);

        message?.Cancel(now);
    }

    public async ValueTask CancelGroup(
        ScheduleGroupId groupId,
        ScheduledMessageTarget? target,
        ScheduleId? exceptId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var tracked = db.ChangeTracker
            .Entries<ScheduledMessageEntry>()
            .Select(entry => entry.Entity)
            .Where(message =>
                message.GroupId == groupId &&
                !message.IsTerminal &&
                (target == null || message.Target == target.Value) &&
                (exceptId == null || message.Id != exceptId.Value))
            .ToArray();

        foreach (var message in tracked)
        {
            message.Cancel(now);
        }

        var trackedIds = tracked
            .Select(message => message.Id)
            .ToList();

        var query = db.Set<ScheduledMessageEntry>()
            .Where(message =>
                message.GroupId == groupId &&
                message.DispatchedAt == null &&
                message.CancelledAt == null &&
                message.FailedAt == null &&
                (exceptId == null || message.Id != exceptId.Value) &&
                !trackedIds.Contains(message.Id));

        if (target is { } scheduledTarget)
        {
            query = query.Where(message =>
                message.Target == scheduledTarget);
        }

        var persisted = await query
            .ToArrayAsync(cancellationToken);

        foreach (var message in persisted)
        {
            message.Cancel(now);
        }
    }

    public async ValueTask<IReadOnlyList<ScheduledMessageEntry>> GetGroup(
        ScheduleGroupId groupId,
        ScheduledMessageTarget? target,
        CancellationToken cancellationToken)
    {
        var query = db.Set<ScheduledMessageEntry>()
            .AsNoTracking()
            .Where(message => message.GroupId == groupId);

        if (target is { } scheduledTarget)
        {
            query = query.Where(message =>
                message.Target == scheduledTarget);
        }

        return await query
            .OrderBy(message => message.DueAt)
            .ThenBy(message => message.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async ValueTask<DateTime?> NextDue(
        DateTime now,
        CancellationToken cancellationToken)
    {
        return await db.Set<ScheduledMessageEntry>()
            .AsNoTracking()
            .Where(message =>
                message.DispatchedAt == null &&
                message.CancelledAt == null &&
                message.FailedAt == null &&
                (message.ClaimedUntil == null || message.ClaimedUntil <= now))
            .Select(message =>
                (DateTime?)(message.NextAttemptAt ?? message.DueAt))
            .OrderBy(value => value)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async ValueTask<IReadOnlyList<OwnedScheduledMessage>> Claim(
        DateTime now,
        int limit,
        TimeSpan lease,
        CancellationToken cancellationToken)
    {
        if (limit <= 0)
        {
            return [];
        }

        if (lease <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lease),
                lease,
                "Scheduling lease must be greater than zero.");
        }

        var claimId = ClaimId.Create();
        var claimedUntil = now.Add(lease);
        var strategy = db.Database.CreateExecutionStrategy();

        var entries = await strategy.ExecuteInTransactionAsync(
            token => Claim(
                db,
                claimId,
                now,
                claimedUntil,
                limit,
                token),
            token => ClaimExists(
                db,
                claimId,
                token),
            cancellationToken);

        return entries
            .Select(message =>
                new OwnedScheduledMessage(
                    message,
                    claimId))
            .ToArray();
    }

    public async ValueTask<bool> Retry(
        OwnedScheduledMessage owned,
        ScheduledMessageFailure failure,
        DateTime now,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken)
    {
        var affected = await Owned(
                db,
                owned).
            ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        message => message.ClaimId,
                        (string?)null)
                    .SetProperty(
                        message => message.ClaimedUntil,
                        (DateTime?)null)
                    .SetProperty(
                        message => message.NextAttemptAt,
                        nextAttemptAt)
                    .SetProperty(
                        message => message.FailureCode,
                        failure.Code)
                    .SetProperty(
                        message => message.FailureMessage,
                        failure.Message)
                    .SetProperty(
                        message => message.ExceptionType,
                        failure.ExceptionType),
                cancellationToken);

        return affected == 1;
    }

    public async ValueTask<bool> Fail(
        OwnedScheduledMessage owned,
        ScheduledMessageFailure failure,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var affected = await Owned(
                db,
                owned).
            ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        message => message.ClaimId,
                        (string?)null)
                    .SetProperty(
                        message => message.ClaimedUntil,
                        (DateTime?)null)
                    .SetProperty(
                        message => message.NextAttemptAt,
                        (DateTime?)null)
                    .SetProperty(
                        message => message.FailedAt,
                        now)
                    .SetProperty(
                        message => message.FailureCode,
                        failure.Code)
                    .SetProperty(
                        message => message.FailureMessage,
                        failure.Message)
                    .SetProperty(
                        message => message.ExceptionType,
                        failure.ExceptionType)
                    .SetProperty(
                        message => message.Version,
                        message => message.Version + 1),
                cancellationToken);

        return affected == 1;
    }

    public async ValueTask<int> Cleanup(
        DateTime before,
        int limit,
        CancellationToken cancellationToken)
    {
        return await db.Set<ScheduledMessageEntry>()
            .Where(message =>
                (message.DispatchedAt != null && message.DispatchedAt < before) ||
                (message.CancelledAt != null && message.CancelledAt < before) ||
                (message.FailedAt != null && message.FailedAt < before))
            .OrderBy(message => message.CreatedAt)
            .Take(limit)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static async Task<ScheduledMessageEntry[]> Claim(
        TDbContext context,
        string claimId,
        DateTime now,
        DateTime claimedUntil,
        int limit,
        CancellationToken cancellationToken)
    {
        var affected = await Available(
                context,
                now)
            .OrderBy(message => message.DueAt)
            .ThenBy(message => message.Id)
            .Take(limit)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        message => message.ClaimId,
                        claimId)
                    .SetProperty(
                        message => message.ClaimedUntil,
                        claimedUntil)
                    .SetProperty(
                        message => message.NextAttemptAt,
                        (DateTime?)null)
                    .SetProperty(
                        message => message.Attempts,
                        message => message.Attempts + 1),
                cancellationToken);

        if (affected == 0)
        {
            return [];
        }

        var entries = await context.Set<ScheduledMessageEntry>()
            .AsNoTracking()
            .Where(message => message.ClaimId == claimId)
            .OrderBy(message => message.DueAt)
            .ThenBy(message => message.Id)
            .ToArrayAsync(cancellationToken);

        if (entries.Length != affected)
        {
            throw new InvalidOperationException(
                $"Claimed {affected} scheduled messages but recovered {entries.Length}.");
        }

        return entries;
    }

    private static Task<bool> ClaimExists(
        DbContext context,
        string claimId,
        CancellationToken cancellationToken)
    {
        return context.Set<ScheduledMessageEntry>()
            .AsNoTracking()
            .AnyAsync(
                message => message.ClaimId == claimId,
                cancellationToken);
    }

    private static IQueryable<ScheduledMessageEntry> Available(
        DbContext context,
        DateTime now)
    {
        return context.Set<ScheduledMessageEntry>()
            .Where(message =>
                message.DispatchedAt == null &&
                message.CancelledAt == null &&
                message.FailedAt == null &&
                message.DueAt <= now &&
                (message.NextAttemptAt == null || message.NextAttemptAt <= now) &&
                (message.ClaimedUntil == null || message.ClaimedUntil <= now));
    }

    private static IQueryable<ScheduledMessageEntry> Owned(
        DbContext context,
        OwnedScheduledMessage owned)
    {
        return context.Set<ScheduledMessageEntry>()
            .Where(message =>
                message.Id == owned.Message.Id &&
                message.ClaimId == owned.ClaimId &&
                message.DispatchedAt == null &&
                message.CancelledAt == null &&
                message.FailedAt == null);
    }

    private static IQueryable<ScheduledMessageEntry> Filter(
        IQueryable<ScheduledMessageEntry> query,
        ScheduleGroupId? groupId,
        ScheduledMessageTarget? target)
    {
        if (groupId is { } group)
        {
            query = query.Where(message =>
                message.GroupId == group);
        }

        if (target is { } scheduledTarget)
        {
            query = query.Where(message =>
                message.Target == scheduledTarget);
        }

        return query;
    }

    private static bool Matches(
        ScheduledMessageEntry message,
        ScheduleGroupId? groupId,
        ScheduledMessageTarget? target)
    {
        return (groupId == null || message.GroupId == groupId) &&
               (target == null || message.Target == target.Value);
    }
}

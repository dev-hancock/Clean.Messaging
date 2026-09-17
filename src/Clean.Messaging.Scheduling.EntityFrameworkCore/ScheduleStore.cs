using Clean.Messaging.Scheduling.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clean.Messaging.Scheduling.EntityFrameworkCore;

internal sealed class ScheduleStore<TDbContext>(
    TDbContext db)
    : IScheduleStore,
      IScheduleReader
    where TDbContext : DbContext
{
    public void Add(
        ScheduleEntry message)
    {
        db.Set<ScheduleEntry>()
            .Add(message);
    }

    public async ValueTask Cancel(
        ScheduleId id,
        ScheduleGroupId? groupId,
        ScheduleTarget? target,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var tracked = db.ChangeTracker
            .Entries<ScheduleEntry>()
            .Select(entry => entry.Entity)
            .SingleOrDefault(message =>
                message.Id == id &&
                Matches(message, groupId, target));

        if (tracked is not null)
        {
            tracked.Cancel(now);
            return;
        }

        var query = db.Set<ScheduleEntry>()
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
        ScheduleTarget? target,
        ScheduleId? exceptId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var tracked = db.ChangeTracker
            .Entries<ScheduleEntry>()
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

        var query = db.Set<ScheduleEntry>()
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

    public async ValueTask<IReadOnlyList<ScheduleEntry>> GetGroup(
        ScheduleGroupId groupId,
        ScheduleTarget? target,
        CancellationToken cancellationToken)
    {
        var query = db.Set<ScheduleEntry>()
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

    async ValueTask<IReadOnlyList<ScheduleStatus>> IScheduleReader.GetGroup(
        ScheduleGroupId groupId,
        ScheduleTarget? target,
        CancellationToken cancellationToken)
    {
        var messages = await GetGroup(
            groupId,
            target,
            cancellationToken);

        return messages
            .Select(message => new ScheduleStatus(
                message.Id,
                message.GroupId,
                message.Target,
                message.Message,
                message.DueAt,
                message.Attempts,
                message.NextAttemptAt,
                message.DispatchedAt,
                message.CancelledAt,
                message.FailedAt,
                message.FailureCode,
                message.FailureMessage))
            .ToArray();
    }

    public async ValueTask<DateTime?> NextDue(
        DateTime now,
        CancellationToken cancellationToken)
    {
        return await db.Set<ScheduleEntry>()
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

    public async ValueTask<IReadOnlyList<OwnedSchedule>> Claim(
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
                new OwnedSchedule(
                    message,
                    claimId))
            .ToArray();
    }

    public async ValueTask<bool> Retry(
        OwnedSchedule owned,
        ScheduleFailure failure,
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
        OwnedSchedule owned,
        ScheduleFailure failure,
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
        return await db.Set<ScheduleEntry>()
            .Where(message =>
                (message.DispatchedAt != null && message.DispatchedAt < before) ||
                (message.CancelledAt != null && message.CancelledAt < before) ||
                (message.FailedAt != null && message.FailedAt < before))
            .OrderBy(message => message.CreatedAt)
            .Take(limit)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static async Task<ScheduleEntry[]> Claim(
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

        var entries = await context.Set<ScheduleEntry>()
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
        return context.Set<ScheduleEntry>()
            .AsNoTracking()
            .AnyAsync(
                message => message.ClaimId == claimId,
                cancellationToken);
    }

    private static IQueryable<ScheduleEntry> Available(
        DbContext context,
        DateTime now)
    {
        return context.Set<ScheduleEntry>()
            .Where(message =>
                message.DispatchedAt == null &&
                message.CancelledAt == null &&
                message.FailedAt == null &&
                message.DueAt <= now &&
                (message.NextAttemptAt == null || message.NextAttemptAt <= now) &&
                (message.ClaimedUntil == null || message.ClaimedUntil <= now));
    }

    private static IQueryable<ScheduleEntry> Owned(
        DbContext context,
        OwnedSchedule owned)
    {
        return context.Set<ScheduleEntry>()
            .Where(message =>
                message.Id == owned.Message.Id &&
                message.ClaimId == owned.ClaimId &&
                message.DispatchedAt == null &&
                message.CancelledAt == null &&
                message.FailedAt == null);
    }

    private static IQueryable<ScheduleEntry> Filter(
        IQueryable<ScheduleEntry> query,
        ScheduleGroupId? groupId,
        ScheduleTarget? target)
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
        ScheduleEntry message,
        ScheduleGroupId? groupId,
        ScheduleTarget? target)
    {
        return (groupId == null || message.GroupId == groupId) &&
               (target == null || message.Target == target.Value);
    }
}

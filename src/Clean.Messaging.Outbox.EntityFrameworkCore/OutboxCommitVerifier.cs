using Clean.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Outbox.EntityFrameworkCore;

internal interface IOutboxCommitVerifier
{
    bool IsCommitted(
        IReadOnlyCollection<EntryKey> keys);

    Task<bool> IsCommittedAsync(
        IReadOnlyCollection<EntryKey> keys,
        CancellationToken cancellationToken);
}

internal sealed class OutboxCommitVerifier<TDbContext>(
    IServiceScopeFactory scopes)
    : IOutboxCommitVerifier
    where TDbContext : DbContext
{
    public bool IsCommitted(
        IReadOnlyCollection<EntryKey> keys)
    {
        ArgumentNullException.ThrowIfNull(
            keys);

        if (keys.Count == 0)
        {
            return false;
        }

        using var scope =
            scopes.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<TDbContext>();

        return Verify(
            db,
            keys);
    }

    public async Task<bool> IsCommittedAsync(
        IReadOnlyCollection<EntryKey> keys,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(
            keys);

        if (keys.Count == 0)
        {
            return false;
        }

        await using var scope =
            scopes.CreateAsyncScope();

        var db = scope.ServiceProvider
            .GetRequiredService<TDbContext>();

        return await VerifyAsync(
            db,
            keys,
            cancellationToken);
    }

    private static bool Verify(
        TDbContext db,
        IReadOnlyCollection<EntryKey> keys)
    {
        var messageIds = keys
            .Select(key => key.MessageId)
            .Distinct()
            .ToArray();

        var committed = db
            .Set<OutboxEntry>()
            .AsNoTracking()
            .Where(entry =>
                messageIds.Contains(entry.MessageId))
            .Select(entry => new
            {
                entry.MessageId,
                entry.TargetId
            })
            .ToArray()
            .Select(entry =>
                new EntryKey(
                    entry.MessageId,
                    entry.TargetId))
            .ToHashSet();

        return keys.All(
            committed.Contains);
    }

    private static async Task<bool> VerifyAsync(
        TDbContext db,
        IReadOnlyCollection<EntryKey> keys,
        CancellationToken cancellationToken)
    {
        var messageIds = keys
            .Select(key => key.MessageId)
            .Distinct()
            .ToArray();

        var rows = await db
            .Set<OutboxEntry>()
            .AsNoTracking()
            .Where(entry =>
                messageIds.Contains(entry.MessageId))
            .Select(entry => new
            {
                entry.MessageId,
                entry.TargetId
            })
            .ToArrayAsync(cancellationToken);

        var committed = rows
            .Select(entry =>
                new EntryKey(
                    entry.MessageId,
                    entry.TargetId))
            .ToHashSet();

        return keys.All(
            committed.Contains);
    }
}
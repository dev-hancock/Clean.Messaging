using Clean.Messaging.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Transactions;

namespace Clean.Messaging.Inbox.EntityFrameworkCore;

internal sealed class InboxStore<TDbContext>(
    TDbContext db,
    IServiceScopeFactory scopes)
    : DurableStore<TDbContext, InboxEntry>(
        db,
        scopes),
      IInboxStore
    where TDbContext : DbContext
{
    public async ValueTask Accept(
        InboxBatch batch,
        CancellationToken cancellationToken)
    {
        EnsureIndependentAdmission();

        await using var scope =
            CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<TDbContext>();

        if (await IsAccepted(
                context,
                batch,
                cancellationToken))
        {
            return;
        }

        await Accept(
            context,
            batch,
            cancellationToken);
    }

    public async ValueTask<ReplayResult> Replay(
        string consumerId,
        string contract,
        DateTime? from,
        DateTime? to,
        int limit,
        CancellationToken cancellationToken)
    {
        EnsureIndependentAdmission();

        await using var scope =
            CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<TDbContext>();

        var admissions = await GetAdmissions(
            context,
            contract,
            from,
            to,
            limit,
            cancellationToken);

        if (admissions.Length == 0)
        {
            return default;
        }

        var existing = await GetExistingEvents(
            context,
            consumerId,
            admissions,
            cancellationToken);

        var entries = admissions
            .Where(admission =>
                !existing.Contains(
                    admission.EventId))
            .Select(admission =>
                CreateEntry(
                    admission,
                    consumerId))
            .ToArray();

        if (entries.Length == 0)
        {
            return new(
                admissions.Length,
                0);
        }

        context
            .Set<InboxEntry>()
            .AddRange(entries);

        try
        {
            await context.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException)
        {
            context.ChangeTracker.Clear();

            var persisted = await GetExistingEvents(
                context,
                consumerId,
                admissions,
                cancellationToken);

            return new(
                admissions.Length,
                persisted.Count - existing.Count);
        }

        return new(
            admissions.Length,
            entries.Length);
    }

    private static async Task Accept(
        TDbContext context,
        InboxBatch batch,
        CancellationToken cancellationToken)
    {
        context
            .Set<InboxAdmission>()
            .Add(
                CreateAdmission(batch));

        if (!batch.IsEmpty)
        {
            context
                .Set<InboxEntry>()
                .AddRange(batch.Entries);
        }

        try
        {
            await context.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException)
        {
            context.ChangeTracker.Clear();

            if (!await IsAccepted(
                    context,
                    batch,
                    cancellationToken))
            {
                throw;
            }
        }
    }

    private static Task<bool> IsAccepted(
        TDbContext context,
        InboxBatch batch,
        CancellationToken cancellationToken)
    {
        return context
            .Set<InboxAdmission>()
            .AsNoTracking()
            .AnyAsync(
                admission =>
                    admission.MessageId == batch.MessageId ||
                    admission.EventId == batch.EventId,
                cancellationToken);
    }

    private static Task<InboxAdmission[]> GetAdmissions(
        TDbContext context,
        string contract,
        DateTime? from,
        DateTime? to,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = context
            .Set<InboxAdmission>()
            .AsNoTracking()
            .Where(admission =>
                admission.Message.Type == contract);

        if (from is not null)
        {
            query = query.Where(admission =>
                admission.CreatedAt >= from);
        }

        if (to is not null)
        {
            query = query.Where(admission =>
                admission.CreatedAt <= to);
        }

        return query
            .OrderBy(admission =>
                admission.CreatedAt)
            .ThenBy(admission =>
                admission.MessageId)
            .Take(limit)
            .ToArrayAsync(
                cancellationToken);
    }

    private static async Task<HashSet<Guid>> GetExistingEvents(
        TDbContext context,
        string consumerId,
        IReadOnlyCollection<InboxAdmission> admissions,
        CancellationToken cancellationToken)
    {
        var eventIds = admissions
            .Select(admission =>
                admission.EventId)
            .ToArray();

        var existing = await context
            .Set<InboxEntry>()
            .AsNoTracking()
            .Where(entry =>
                entry.ConsumerId == consumerId &&
                eventIds.Contains(entry.EventId))
            .Select(entry =>
                entry.EventId)
            .ToArrayAsync(
                cancellationToken);

        return existing.ToHashSet();
    }

    private static InboxAdmission CreateAdmission(
        InboxBatch batch)
    {
        var first = batch.Entries
            .FirstOrDefault();

        if (first is null)
        {
            throw new InvalidOperationException(
                "An inbox admission requires the original message envelope.");
        }

        return new()
        {
            MessageId = batch.MessageId,
            EventId = batch.EventId,
            Message = batch.Message,
            CorrelationId = batch.CorrelationId,
            CausationId = batch.CausationId,
            CreatedAt = batch.CreatedAt
        };
    }

    private static InboxEntry CreateEntry(
        InboxAdmission admission,
        string consumerId)
    {
        return new()
        {
            MessageId = admission.MessageId,
            EventId = admission.EventId,
            ConsumerId = consumerId,
            Message = admission.Message,
            CorrelationId = admission.CorrelationId,
            CausationId = admission.CausationId,
            CreatedAt = admission.CreatedAt
        };
    }

    private static void EnsureIndependentAdmission()
    {
        if (Transaction.Current is not null)
        {
            throw new InvalidOperationException(
                "Inbox admission cannot run inside an ambient transaction.");
        }
    }
}
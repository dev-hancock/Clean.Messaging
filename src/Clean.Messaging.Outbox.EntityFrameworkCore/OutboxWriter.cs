using Clean.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Clean.Messaging.Outbox.EntityFrameworkCore;

internal interface IOutboxWriter
{
    void Stage(
        DbContext db,
        OutboxBatch batch);
}

internal sealed class OutboxWriter
    : IOutboxWriter
{
    public void Stage(
        DbContext db,
        OutboxBatch batch)
    {
        if (batch.IsEmpty)
        {
            return;
        }

        var consumers = db.ChangeTracker
            .Entries<OutboxEntry>()
            .Where(entry =>
                entry.State != EntityState.Deleted &&
                entry.Entity.EventId == batch.EventId)
            .Select(entry => entry.Entity.ConsumerId)
            .ToHashSet(StringComparer.Ordinal);

        var missing = batch.Entries
            .Where(entry =>
                !consumers.Contains(entry.ConsumerId))
            .ToArray();

        if (missing.Length == 0)
        {
            return;
        }

        db.Set<OutboxEntry>()
            .AddRange(missing);
    }
}

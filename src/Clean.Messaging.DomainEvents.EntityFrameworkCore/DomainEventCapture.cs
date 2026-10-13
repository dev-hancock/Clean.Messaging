using Clean.Messaging.Outbox;
using Clean.Messaging.Outbox.EntityFrameworkCore;
using Clean.Messaging.Outbox.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clean.Messaging.DomainEvents.EntityFrameworkCore;

internal sealed class DomainEventCapture(
    OutboxEntryFactory entries,
    IOutboxWriter writer,
    OutboxTracker tracker)
    : ISaveCapture
{
    public void Capture(
        DbContext db)
    {
        var aggregates = db.ChangeTracker
            .Entries()
            .Select(entry => entry.Entity)
            .OfType<IDomainEventSource>()
            .ToArray();

        foreach (var aggregate in aggregates)
        {
            Capture(
                db,
                aggregate);
        }
    }

    private void Capture(
        DbContext db,
        IDomainEventSource aggregate)
    {
        foreach (var @event in aggregate.Events)
        {
            if (!tracker.Capture<DomainEventCapture>(
                    db,
                    @event.Id))
            {
                continue;
            }

            tracker.AcknowledgeAfterCommit(
                db,
                @event.Id,
                () => aggregate.Acknowledge(
                    @event.Id));

            writer.Stage(
                db,
                entries.Create(
                    new OutboxMessage<object>(
                        @event.Id,
                        @event.Message)));
        }
    }
}

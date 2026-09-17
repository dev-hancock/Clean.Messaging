using Clean.Messaging.Ddd;
using Clean.Messaging.Outbox;
using Clean.Messaging.Outbox.EntityFrameworkCore;
using Clean.Messaging.Outbox.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clean.Messaging.Ddd.EntityFrameworkCore;

internal sealed class DomainEventCapture(
    OutboxEntryFactory entries,
    IOutboxWriter writer,
    OutboxTracker tracker)
    : IOutboxCapture
{
    public void Capture(
        DbContext db)
    {
        var aggregates = db.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
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
        AggregateRoot aggregate)
    {
        foreach (var message in aggregate.Events)
        {
            var eventId = message.EventId;

            if (!tracker.Capture(
                    db,
                    eventId,
                    () => aggregate.Acknowledge(eventId)))
            {
                continue;
            }

            writer.Stage(
                db,
                entries.Create(
                    new OutboxMessage<DomainEvent>(
                        eventId,
                        message)));
        }
    }
}

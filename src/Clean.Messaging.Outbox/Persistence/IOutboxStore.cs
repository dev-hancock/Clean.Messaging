using Clean.Messaging.Persistence;

namespace Clean.Messaging.Outbox.Persistence;

public sealed record OutboxBatch(
    Guid MessageId,
    Guid EventId,
    IReadOnlyList<OutboxEntry> Entries)
{
    public bool IsEmpty => Entries.Count == 0;
}

public interface IOutboxStore : IDurableStore<OutboxEntry>
{
    void Stage(OutboxBatch batch);
}
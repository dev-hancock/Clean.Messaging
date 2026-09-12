namespace Clean.Messaging.Outbox;

internal sealed class Outbox(
    OutboxEntryFactory entries,
    IOutboxStore store)
    : IOutbox
{
    public void Enqueue<T>(
        OutboxMessage<T> message)
        where T : notnull
    {
        store.Stage(
            entries.Create(message));
    }
}
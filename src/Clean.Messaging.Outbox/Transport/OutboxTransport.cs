using Clean.Messaging.Outbox.Delivery;
using Clean.Messaging.Outbox.Persistence;
using Clean.Messaging.Persistence;

namespace Clean.Messaging.Outbox.Transport;

internal sealed class OutboxTransport<TTransport>(
    TTransport transport,
    IOutboxStore store,
    TimeProvider time)
    : IOutboxDelivery
    where TTransport : class, IOutboxTransport
{
    public string Transport =>
        transport.Name;

    public async ValueTask<bool> Dispatch(
        OwnedEntry<OutboxEntry> owned,
        CancellationToken cancellationToken)
    {
        var entry =
            owned.Entry;

        var envelope =
            new OutboxEnvelope(
                entry.MessageId,
                entry.EventId,
                entry.TargetId,
                entry.Destination,
                entry.Message,
                entry.CorrelationId,
                entry.CausationId);

        await transport.Dispatch(
            envelope,
            cancellationToken);

        return await store.Complete(
            entry.Key,
            owned.ClaimId,
            time.GetUtcNow().UtcDateTime,
            cancellationToken);
    }
}

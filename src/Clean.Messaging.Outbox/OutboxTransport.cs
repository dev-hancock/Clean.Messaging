using Clean.Messaging.Persistence;
using System.Text;

namespace Clean.Messaging.Outbox;

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

        var dispatch =
            new OutboxDispatch(
                entry.MessageId,
                entry.EventId,
                entry.ConsumerId,
                entry.Destination,
                entry.Message.Type,
                Encoding.UTF8.GetBytes(
                    entry.Message.Value),
                entry.CorrelationId,
                entry.CausationId);

        await transport.Dispatch(
            dispatch,
            cancellationToken);

        return await store.Complete(
            entry.Key,
            owned.ClaimId,
            time.GetUtcNow().UtcDateTime,
            cancellationToken);
    }
}
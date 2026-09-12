using Clean.Messaging.Persistence;

namespace Clean.Messaging.Outbox;

internal interface IOutboxDelivery
{
    string Transport { get; }

    ValueTask<bool> Dispatch(
        OwnedEntry<OutboxEntry> owned,
        CancellationToken cancellationToken);
}
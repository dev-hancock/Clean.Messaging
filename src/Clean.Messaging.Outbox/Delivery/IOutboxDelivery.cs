using Clean.Messaging.Outbox.Persistence;
using Clean.Messaging.Persistence;

namespace Clean.Messaging.Outbox.Delivery;

public interface IOutboxDelivery
{
    string Transport { get; }

    ValueTask<bool> Dispatch(
        OwnedEntry<OutboxEntry> owned,
        CancellationToken cancellationToken);
}
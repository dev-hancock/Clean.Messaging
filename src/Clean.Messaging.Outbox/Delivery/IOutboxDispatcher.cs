using Clean.Messaging.Outbox.Persistence;
using Clean.Messaging.Persistence;

namespace Clean.Messaging.Outbox.Delivery;

public interface IOutboxDispatcher
{
    ValueTask<bool> Dispatch(
        OwnedEntry<OutboxEntry> owned,
        CancellationToken cancellationToken);
}

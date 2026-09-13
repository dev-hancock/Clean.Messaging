using Clean.Messaging.Persistence;

namespace Clean.Messaging.Outbox;

public interface IOutboxDispatcher
{
    ValueTask<bool> Dispatch(
        OwnedEntry<OutboxEntry> owned,
        CancellationToken cancellationToken);
}

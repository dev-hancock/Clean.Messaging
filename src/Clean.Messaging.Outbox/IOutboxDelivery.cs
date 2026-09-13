using Clean.Messaging.Persistence;

namespace Clean.Messaging.Outbox;

public interface IOutboxDelivery
{
    string Transport { get; }

    ValueTask<bool> Dispatch(
        OwnedEntry<OutboxEntry> owned,
        CancellationToken cancellationToken);
}
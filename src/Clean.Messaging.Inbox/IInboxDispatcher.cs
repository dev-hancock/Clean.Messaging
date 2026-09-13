using Clean.Messaging.Persistence;

namespace Clean.Messaging.Inbox;

public interface IInboxDispatcher
{
    ValueTask<bool> Dispatch(
        OwnedEntry<InboxEntry> owned,
        CancellationToken cancellationToken);
}

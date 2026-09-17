using Clean.Messaging.Inbox.Persistence;
using Clean.Messaging.Persistence;

namespace Clean.Messaging.Inbox.Dispatching;

public interface IInboxDispatcher
{
    ValueTask<bool> Dispatch(
        OwnedEntry<InboxEntry> owned,
        CancellationToken cancellationToken);
}
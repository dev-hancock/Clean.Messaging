using Clean.Messaging.Persistence;

namespace Clean.Messaging.Inbox;

internal interface IInboxDispatcher
{
    ValueTask<bool> Dispatch(
        OwnedEntry<InboxEntry> owned,
        CancellationToken cancellationToken);
}

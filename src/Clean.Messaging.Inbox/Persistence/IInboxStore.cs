using Clean.Messaging.Inbox.Runtime;
using Clean.Messaging.Persistence;
using Clean.Messaging.Serialization;

namespace Clean.Messaging.Inbox.Persistence;

public sealed record InboxBatch(
    Guid MessageId,
    Guid EventId,
    MessageData Message,
    Guid? CorrelationId,
    Guid? CausationId,
    DateTime CreatedAt,
    IReadOnlyList<InboxEntry> Entries)
{
    public bool IsEmpty =>
        Entries.Count == 0;
}

public interface IInboxStore :
    IDurableStore<InboxEntry>
{
    ValueTask Accept(
        InboxBatch batch,
        CancellationToken cancellationToken);

    ValueTask<ReplayResult> Replay(
        string consumerId,
        string contract,
        DateTime? from,
        DateTime? to,
        int limit,
        CancellationToken cancellationToken);
}
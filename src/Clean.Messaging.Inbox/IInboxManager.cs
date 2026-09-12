using Clean.Messaging.Abstractions;

namespace Clean.Messaging.Inbox;

public interface IInboxManager
{
    ValueTask<IReadOnlyList<DeadLetter>> GetDeadLetters(
        int limit = 100,
        CancellationToken cancellationToken = default);

    ValueTask<RequeueResult> Requeue(
        IReadOnlyCollection<EntryKey> entries,
        CancellationToken cancellationToken = default);

    ValueTask<int> Discard(
        IReadOnlyCollection<EntryKey> entries,
        CancellationToken cancellationToken = default);

    ValueTask<ReplayResult> Replay<TMessage>(
        string consumerId,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 1000,
        CancellationToken cancellationToken = default)
        where TMessage : notnull;
}
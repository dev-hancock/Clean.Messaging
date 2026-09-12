using Clean.Messaging.Abstractions;

namespace Clean.Messaging.Outbox;

public interface IOutboxManager
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
}
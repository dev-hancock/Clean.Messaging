using Clean.Messaging.Abstractions;
using Clean.Messaging.DeadLetters;
using Clean.Messaging.Outbox.Persistence;

namespace Clean.Messaging.Outbox.Runtime;

public sealed class OutboxManager(
    IDeadLetterStore<OutboxEntry> store,
    OutboxSignal signal)
    : IOutboxManager
{
    public ValueTask<IReadOnlyList<DeadLetter>> GetDeadLetters(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            limit);

        return store.Get(
            limit,
            cancellationToken);
    }

    public async ValueTask<RequeueResult> Requeue(
        IReadOnlyCollection<EntryKey> entries,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var requeued = 0;

        foreach (var entry in entries)
        {
            if (await store.Requeue(
                    entry,
                    cancellationToken))
            {
                requeued++;
            }
        }

        if (requeued > 0)
        {
            signal.Wake();
        }

        return new(
            entries.Count,
            requeued);
    }

    public async ValueTask<int> Discard(
        IReadOnlyCollection<EntryKey> entries,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var discarded = 0;

        foreach (var entry in entries)
        {
            if (await store.Discard(
                    entry,
                    cancellationToken))
            {
                discarded++;
            }
        }

        return discarded;
    }
}
using Clean.Messaging.Abstractions;
using Clean.Messaging.Persistence;

namespace Clean.Messaging.DeadLetters;

internal interface IDeadLetterStore<TEntry>
    where TEntry : class, IDurableEntry
{
    ValueTask<IReadOnlyList<DeadLetter>> Get(
        int limit,
        CancellationToken cancellationToken);

    ValueTask<bool> Requeue(
        EntryKey key,
        CancellationToken cancellationToken);

    ValueTask<bool> Discard(
        EntryKey key,
        CancellationToken cancellationToken);
}

using Clean.Messaging.Persistence;

namespace Clean.Messaging.Processing;

internal interface IWorkProcessor<TEntry>
{
    ValueTask Process(
        OwnedEntry<TEntry> owned,
        CancellationToken cancellationToken);
}

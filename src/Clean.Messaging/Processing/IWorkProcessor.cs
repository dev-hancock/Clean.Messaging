using Clean.Messaging.Persistence;

namespace Clean.Messaging.Processing;

public interface IWorkProcessor<TEntry>
{
    ValueTask Process(
        OwnedEntry<TEntry> owned,
        CancellationToken cancellationToken);
}

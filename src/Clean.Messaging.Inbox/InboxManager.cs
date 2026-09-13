using Clean.Messaging.Abstractions;
using Clean.Messaging.DeadLetters;
using Clean.Messaging.Serialization;

namespace Clean.Messaging.Inbox;

public sealed class InboxManager(
    IInboxStore inbox,
    IDeadLetterStore<InboxEntry> deadLetters,
    IMessageContractRegistry contracts,
    InboxSignal signal)
    : IInboxManager
{
    public ValueTask<IReadOnlyList<DeadLetter>> GetDeadLetters(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            limit);

        return deadLetters.Get(
            limit,
            cancellationToken);
    }

    public async ValueTask<RequeueResult> Requeue(
        IReadOnlyCollection<EntryKey> entries,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            entries);

        var requeued = 0;

        foreach (var entry in entries)
        {
            if (await deadLetters.Requeue(
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
        ArgumentNullException.ThrowIfNull(
            entries);

        var discarded = 0;

        foreach (var entry in entries)
        {
            if (await deadLetters.Discard(
                    entry,
                    cancellationToken))
            {
                discarded++;
            }
        }

        return discarded;
    }

    public async ValueTask<ReplayResult> Replay<TMessage>(
        string consumerId,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 1000,
        CancellationToken cancellationToken = default)
        where TMessage : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            consumerId);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            limit);

        if (from is not null &&
            to is not null &&
            from > to)
        {
            throw new ArgumentException(
                "'from' cannot be later than 'to'.");
        }

        var contract = contracts.GetContract(
            typeof(TMessage));

        var result = await inbox.Replay(
            consumerId,
            contract,
            from,
            to,
            limit,
            cancellationToken);

        if (result.Replayed > 0)
        {
            signal.Wake();
        }

        return result;
    }
}
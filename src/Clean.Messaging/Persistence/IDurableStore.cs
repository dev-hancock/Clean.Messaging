using Clean.Messaging.Abstractions;
using Clean.Messaging.Failures;

namespace Clean.Messaging.Persistence;

public sealed record OwnedEntry<TEntry>(
    TEntry Entry,
    string ClaimId);

public interface IDurableStore<TEntry>
{
    ValueTask<IReadOnlyList<OwnedEntry<TEntry>>> Claim(
        DateTime now,
        int limit,
        TimeSpan lease,
        CancellationToken cancellationToken);

    ValueTask<bool> Renew(
        EntryKey key,
        string claimId,
        DateTime now,
        DateTime claimedUntil,
        CancellationToken cancellationToken);

    ValueTask<bool> Complete(
        EntryKey key,
        string claimId,
        DateTime now,
        CancellationToken cancellationToken);

    ValueTask<bool> Retry(
        EntryKey key,
        string claimId,
        MessageFailure failure,
        DateTime now,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken);

    ValueTask<bool> DeadLetter(
        EntryKey key,
        string claimId,
        MessageFailure failure,
        DateTime now,
        CancellationToken cancellationToken);

    ValueTask<int> Cleanup(
        DateTime before,
        int limit,
        CancellationToken cancellationToken);
}
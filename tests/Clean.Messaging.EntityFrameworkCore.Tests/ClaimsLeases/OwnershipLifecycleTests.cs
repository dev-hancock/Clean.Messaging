using Clean.Messaging.Abstractions;
using Clean.Messaging.DeadLetters;
using Clean.Messaging.EntityFrameworkCore.Tests.Infrastructure;
using Clean.Messaging.Failures;
using Clean.Messaging.Inbox.Persistence;
using Clean.Messaging.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.EntityFrameworkCore.Tests.ClaimsLeases;

[ClassDataSource<PostgreSqlServer>(Shared = SharedType.PerTestSession)]
public sealed class OwnershipLifecycleTests(PostgreSqlServer server)
{
    [Test]
    [Arguments(OwnedOperation.Complete)]
    [Arguments(OwnedOperation.Retry)]
    [Arguments(OwnedOperation.DeadLetter)]
    [Arguments(OwnedOperation.Renew)]
    [Property("TestCaseId", "CLAIM-002-AC-01")]
    public async Task Update_ObsoleteOwner_PreservesSuccessorOwnership(OwnedOperation operation)
    {
        await using var database = await server.CreateDatabase();
        var entry = database.CreateEntry("billing");
        await database.Save(entry);
        await using var firstScope = database.CreateScope();
        var firstStore = firstScope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var first = (await firstStore.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        var later = TestDatabase.Now.Add(TestDatabase.Lease).AddSeconds(1);
        await using var successorScope = database.CreateScope();
        var successorStore = successorScope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var successor = (await successorStore.Claim(later, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        var expected = EntryState.From(successor.Entry);

        var accepted = await Update(firstStore, operation, entry.Key, first.ClaimId, later);

        await Assert.That(accepted).IsFalse();
        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(expected);
    }

    [Test]
    [MatrixDataSource]
    [Property("TestCaseId", "CLAIM-002-AC-02")]
    public async Task Update_TokenForDifferentTarget_PreservesBothTargets(
        [Matrix] OwnedOperation operation, [Matrix] KeyDifference difference)
    {
        await using var database = await server.CreateDatabase();
        var firstEntry = database.CreateEntry("billing");
        await database.Save(firstEntry);
        await using var firstScope = database.CreateScope();
        var firstStore = firstScope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var first = (await firstStore.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        var other = difference == KeyDifference.MessageId
            ? database.CreateEntry(firstEntry.TargetId)
            : database.CreateEntry("audit", firstEntry.EventId, firstEntry.MessageId);
        await database.Save(other);
        await using var otherScope = database.CreateScope();
        var otherStore = otherScope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var second = (await otherStore.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        var expected = new[] { EntryState.From(first.Entry), EntryState.From(second.Entry) };

        var accepted = await Update(firstStore, operation, other.Key, first.ClaimId, TestDatabase.Now);

        await Assert.That(accepted).IsFalse();
        await Assert.That((await database.ReadEntries()).Select(EntryState.From).ToArray()).IsEquivalentTo(expected);
    }

    [Test]
    [MatrixDataSource]
    [Property("TestCaseId", "CLAIM-002-AC-03")]
    public async Task Update_SuccessorAlreadySettled_PreservesSuccessorOutcome(
        [Matrix] OwnedOperation operation,
        [Matrix(OwnedOperation.Complete, OwnedOperation.DeadLetter)] OwnedOperation successorSettlement)
    {
        await using var database = await server.CreateDatabase();
        var entry = database.CreateEntry("billing");
        await database.Save(entry);
        await using var firstScope = database.CreateScope();
        var firstStore = firstScope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var first = (await firstStore.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        var later = TestDatabase.Now.Add(TestDatabase.Lease).AddSeconds(1);
        await using var successorScope = database.CreateScope();
        var successorStore = successorScope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var successor = (await successorStore.Claim(later, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        await Assert.That(await Update(successorStore, successorSettlement, entry.Key, successor.ClaimId, later)).IsTrue();
        var expected = EntryState.From((await database.ReadEntries()).Single());

        var accepted = await Update(firstStore, operation, entry.Key, first.ClaimId, later);

        await Assert.That(accepted).IsFalse();
        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(expected);
    }

    [Test]
    [MatrixDataSource]
    [Property("TestCaseId", "CLAIM-003-AC-02")]
    public async Task Update_InactiveTargetWithMatchingUnexpiredToken_PreservesRetainedState(
        [Matrix] OwnedOperation operation, [Matrix] SettledState state)
    {
        await using var database = await server.CreateDatabase();
        var entry = database.CreateEntry("billing");
        var claimId = Guid.NewGuid().ToString("N");
        entry.ClaimId = claimId;
        entry.ClaimedUntil = TestDatabase.Now.Add(TestDatabase.Lease);
        entry.NextAttemptAt = state == SettledState.RetryWaiting ? TestDatabase.Now.AddHours(1) : null;
        entry.ProcessedAt = state == SettledState.Completed ? TestDatabase.Now.AddMinutes(-1) : null;
        entry.DeadLetteredAt = state == SettledState.DeadLettered ? TestDatabase.Now.AddMinutes(-1) : null;
        entry.DiscardedAt = state == SettledState.Discarded ? TestDatabase.Now.AddMinutes(-1) : null;
        await database.Save(entry);
        var expected = EntryState.From((await database.ReadEntries()).Single());
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();

        var accepted = await Update(store, operation, entry.Key, claimId, TestDatabase.Now);

        await Assert.That(accepted).IsFalse();
        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(expected);
    }

    [Test]
    [Property("TestCaseId", "CLAIM-003-AC-03")]
    public async Task Renew_CurrentOwner_PersistsLaterExpiryWithoutChangingAttempt()
    {
        await using var database = await server.CreateDatabase();
        var entry = database.CreateEntry("billing");
        await database.Save(entry);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var owned = (await store.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        var laterExpiry = owned.Entry.ClaimedUntil!.Value.AddMinutes(1);
        var expected = EntryState.From(owned.Entry) with { ClaimedUntil = laterExpiry };

        var renewed = await store.Renew(entry.Key, owned.ClaimId, TestDatabase.Now.AddSeconds(30), laterExpiry, CancellationToken.None);

        await Assert.That(renewed).IsTrue();
        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(expected);
    }

    [Test]
    [Arguments(OwnedOperation.Complete)]
    [Arguments(OwnedOperation.Retry)]
    [Arguments(OwnedOperation.DeadLetter)]
    [Property("TestCaseId", "CLAIM-003-AC-04")]
    public async Task Update_CurrentOwner_PersistsSettlementAndReleasesOwnership(OwnedOperation operation)
    {
        await using var database = await server.CreateDatabase();
        var entry = database.CreateEntry("billing");
        await database.Save(entry);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var owned = (await store.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        var failure = CreateFailure(TestDatabase.Now, owned.Entry.Attempts);
        var expected = EntryState.From(owned.Entry) with { ClaimId = null, ClaimedUntil = null };
        expected = operation == OwnedOperation.Complete
            ? expected with { ProcessedAt = TestDatabase.Now }
            : expected with
            {
                NextAttemptAt = operation == OwnedOperation.Retry ? TestDatabase.Now.AddMinutes(5) : null,
                DeadLetteredAt = operation == OwnedOperation.DeadLetter ? TestDatabase.Now : null,
                FailureCategory = failure.Category, FailureCode = failure.Code, ExceptionType = failure.ExceptionType,
                FailureMessage = failure.Message, LastFailedAt = failure.FailedAt, LastFailedAttempt = failure.Attempt
            };

        var accepted = operation switch
        {
            OwnedOperation.Complete => await store.Complete(entry.Key, owned.ClaimId, TestDatabase.Now, CancellationToken.None),
            OwnedOperation.Retry => await store.Retry(entry.Key, owned.ClaimId, failure, TestDatabase.Now, TestDatabase.Now.AddMinutes(5), CancellationToken.None),
            _ => await store.DeadLetter(entry.Key, owned.ClaimId, failure, TestDatabase.Now, CancellationToken.None)
        };

        await Assert.That(accepted).IsTrue();
        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(expected);
    }

    private static ValueTask<bool> Update(IDurableStore<InboxEntry> store, OwnedOperation operation, EntryKey key, string claimId, DateTime now)
    {
        var failure = CreateFailure(now, 1);
        return operation switch
        {
            OwnedOperation.Complete => store.Complete(key, claimId, now, CancellationToken.None),
            OwnedOperation.Retry => store.Retry(key, claimId, failure, now, now.AddMinutes(5), CancellationToken.None),
            OwnedOperation.DeadLetter => store.DeadLetter(key, claimId, failure, now, CancellationToken.None),
            OwnedOperation.Renew => store.Renew(key, claimId, now, now.AddMinutes(3), CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
    }

    private static MessageFailure CreateFailure(DateTime now, int attempt) => new(
        FailureCategory.Transient, "tests.consumer-unavailable", typeof(InvalidOperationException).FullName!,
        "The consumer was unavailable.", now, attempt);

    public enum OwnedOperation { Complete, Retry, DeadLetter, Renew }
    public enum KeyDifference { MessageId, TargetId }
    public enum SettledState { RetryWaiting, Completed, DeadLettered, Discarded }
}

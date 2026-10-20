using Clean.Messaging.DeadLetters;
using Clean.Messaging.EntityFrameworkCore.Tests.Infrastructure;
using Clean.Messaging.Failures;
using Clean.Messaging.Inbox.Persistence;
using Clean.Messaging.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.EntityFrameworkCore.Tests.Persistence;

[ClassDataSource<PostgreSqlServer>(Shared = SharedType.PerTestSession)]
public sealed class StoreBoundaryTests(PostgreSqlServer server)
{
    [Test]
    [Arguments(0, 60)]
    [Arguments(-1, 60)]
    [Arguments(1, 0)]
    [Arguments(1, -1)]
    [Property("TestCaseId", "PERSIST-003-AC-01")]
    public async Task Claim_NonpositiveLimitOrLease_RejectsInvalidBudget(int limit, int leaseSeconds)
    {
        await using var database = await server.CreateDatabase();
        var entry = database.CreateEntry("billing");
        await database.Save(entry);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();

        await Assert.That(async () => await store.Claim(TestDatabase.Now, limit, TimeSpan.FromSeconds(leaseSeconds), CancellationToken.None))
            .Throws<ArgumentOutOfRangeException>();

        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(EntryState.From(entry));
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Property("TestCaseId", "PERSIST-003-AC-02")]
    public async Task Cleanup_NonpositiveLimit_RejectsUnboundedDeletion(int limit)
    {
        await using var database = await server.CreateDatabase();
        var entry = database.CreateEntry("billing");
        entry.ProcessedAt = TestDatabase.Now.AddDays(-1);
        await database.Save(entry);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();

        await Assert.That(async () => await store.Cleanup(TestDatabase.Now, limit, CancellationToken.None))
            .Throws<ArgumentOutOfRangeException>();

        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(EntryState.From(entry));
    }

    [Test]
    [Arguments(StoreOperation.Claim)]
    [Arguments(StoreOperation.Renew)]
    [Arguments(StoreOperation.Complete)]
    [Arguments(StoreOperation.Retry)]
    [Arguments(StoreOperation.DeadLetter)]
    [Arguments(StoreOperation.Cleanup)]
    [Arguments(StoreOperation.GetDeadLetters)]
    [Arguments(StoreOperation.Requeue)]
    [Arguments(StoreOperation.Discard)]
    [Property("TestCaseId", "PERSIST-003-AC-03")]
    public async Task Execute_PreCancelledToken_PropagatesCancellationInsteadOfSuccess(StoreOperation operation)
    {
        await using var database = await server.CreateDatabase();
        var entry = database.CreateEntry("billing");
        await database.Save(entry);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var management = scope.ServiceProvider.GetRequiredService<IDeadLetterStore<InboxEntry>>();
        var owned = (await store.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        var failure = new MessageFailure(FailureCategory.Transient, "tests.unavailable", typeof(InvalidOperationException).FullName!,
            "The consumer was unavailable.", TestDatabase.Now, owned.Entry.Attempts);
        if (operation is StoreOperation.GetDeadLetters or StoreOperation.Requeue or StoreOperation.Discard)
        {
            await Assert.That(await store.DeadLetter(entry.Key, owned.ClaimId, failure, TestDatabase.Now, CancellationToken.None)).IsTrue();
        }
        else if (operation == StoreOperation.Cleanup)
        {
            await Assert.That(await store.Complete(entry.Key, owned.ClaimId, TestDatabase.Now, CancellationToken.None)).IsTrue();
        }
        var expected = EntryState.From((await database.ReadEntries()).Single());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var token = cancellation.Token;

        await Assert.That(async () =>
        {
            switch (operation)
            {
                case StoreOperation.Claim:
                    await store.Claim(TestDatabase.Now.AddMinutes(5), 1, TestDatabase.Lease, token);
                    break;
                case StoreOperation.Renew:
                    await store.Renew(entry.Key, owned.ClaimId, TestDatabase.Now, TestDatabase.Now.AddMinutes(3), token);
                    break;
                case StoreOperation.Complete:
                    await store.Complete(entry.Key, owned.ClaimId, TestDatabase.Now, token);
                    break;
                case StoreOperation.Retry:
                    await store.Retry(entry.Key, owned.ClaimId, failure, TestDatabase.Now, TestDatabase.Now.AddMinutes(5), token);
                    break;
                case StoreOperation.DeadLetter:
                    await store.DeadLetter(entry.Key, owned.ClaimId, failure, TestDatabase.Now, token);
                    break;
                case StoreOperation.Cleanup:
                    await store.Cleanup(TestDatabase.Now.AddDays(1), 1, token);
                    break;
                case StoreOperation.GetDeadLetters:
                    await management.Get(1, token);
                    break;
                case StoreOperation.Requeue:
                    await management.Requeue(entry.Key, token);
                    break;
                case StoreOperation.Discard:
                    await management.Discard(entry.Key, token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }).Throws<OperationCanceledException>();

        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(expected);
    }

    public enum StoreOperation { Claim, Renew, Complete, Retry, DeadLetter, Cleanup, GetDeadLetters, Requeue, Discard }
}

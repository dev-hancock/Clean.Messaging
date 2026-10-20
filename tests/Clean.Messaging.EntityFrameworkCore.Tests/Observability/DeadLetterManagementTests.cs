using System.Transactions;
using Clean.Messaging.Abstractions;
using Clean.Messaging.DeadLetters;
using Clean.Messaging.EntityFrameworkCore.Tests.Infrastructure;
using Clean.Messaging.Failures;
using Clean.Messaging.Inbox.Persistence;
using Clean.Messaging.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.EntityFrameworkCore.Tests.Observability;

[ClassDataSource<PostgreSqlServer>(Shared = SharedType.PerTestSession)]
public sealed class DeadLetterManagementTests(PostgreSqlServer server)
{
    [Test]
    [Property("TestCaseId", "OPS-001-AC-01")]
    public async Task Get_MixedRetainedTargets_ReturnsLimitedOrderedDiagnosticProjection()
    {
        await using var database = await server.CreateDatabase();
        var oldest = CreateDeadLetter(database, "oldest", TestDatabase.Now.AddHours(-3), 5);
        var middle = CreateDeadLetter(database, "middle", TestDatabase.Now.AddHours(-2), 3);
        var newest = CreateDeadLetter(database, "newest", TestDatabase.Now.AddHours(-1), 1);
        var discarded = CreateDeadLetter(database, "discarded", TestDatabase.Now.AddHours(-4), 2);
        discarded.DiscardedAt = TestDatabase.Now;
        var completed = database.CreateEntry("completed");
        completed.ProcessedAt = TestDatabase.Now.AddHours(-5);
        var pending = database.CreateEntry("pending");
        var retrying = database.CreateEntry("retrying");
        retrying.Attempts = 2;
        retrying.NextAttemptAt = TestDatabase.Now.AddMinutes(10);
        var claimed = database.CreateEntry("claimed");
        claimed.ClaimId = "active-worker";
        claimed.ClaimedUntil = TestDatabase.Now.Add(TestDatabase.Lease);
        await database.Save(newest, discarded, completed, middle, pending, oldest, retrying, claimed);
        var before = (await database.ReadEntries()).Select(EntryState.From).ToArray();
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDeadLetterStore<InboxEntry>>();

        var listed = await store.Get(2, CancellationToken.None);

        await Assert.That(listed.Count).IsEqualTo(2);
        await Assert.That(listed[0]).IsEqualTo(Project(oldest));
        await Assert.That(listed[1]).IsEqualTo(Project(middle));
        await Assert.That((await database.ReadEntries()).Select(EntryState.From).ToArray()).IsEquivalentTo(before);
    }

    [Test]
    [Property("TestCaseId", "OPS-001-AC-02")]
    public async Task Requeue_DeadLetterWithFailureHistory_RestoresImmediateEligibilityAndPreservesHistory()
    {
        await using var database = await server.CreateDatabase();
        var entry = database.CreateEntry("failed-target");
        entry.Attempts = 3;
        await database.Save(entry);
        await using var scope = database.CreateScope();
        var durable = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var store = scope.ServiceProvider.GetRequiredService<IDeadLetterStore<InboxEntry>>();
        var owned = (await durable.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        var failure = new MessageFailure(FailureCategory.Permanent, "work.invalid", typeof(InvalidOperationException).FullName!,
            "The retained work could not be handled.", TestDatabase.Now, owned.Entry.Attempts);
        await Assert.That(await durable.DeadLetter(entry.Key, owned.ClaimId, failure, TestDatabase.Now, CancellationToken.None)).IsTrue();
        var before = EntryState.From((await database.ReadEntries()).Single());

        var requeued = await store.Requeue(entry.Key, CancellationToken.None);

        await Assert.That(requeued).IsTrue();
        var after = (await database.ReadEntries()).Single();
        await Assert.That(EntryState.From(after)).IsEqualTo(before with
        {
            Attempts = 0,
            ClaimId = null,
            ClaimedUntil = null,
            NextAttemptAt = null,
            ProcessedAt = null,
            DeadLetteredAt = null,
            DiscardedAt = null,
            FailureCategory = failure.Category,
            FailureCode = failure.Code,
            ExceptionType = failure.ExceptionType,
            FailureMessage = failure.Message,
            LastFailedAt = failure.FailedAt,
            LastFailedAttempt = failure.Attempt
        });
        await Assert.That((await store.Get(10, CancellationToken.None)).Count).IsEqualTo(0);
        var immediatelyAvailable = await durable.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None);
        await Assert.That(immediatelyAvailable.Count).IsEqualTo(1);
        await Assert.That(immediatelyAvailable[0].Entry.Key).IsEqualTo(entry.Key);
    }

    [Test]
    [Property("TestCaseId", "OPS-001-AC-03")]
    public async Task Discard_RetainedDeadLetter_RemovesListingButPreservesStoredOutcomeAndDiagnostics()
    {
        await using var database = await server.CreateDatabase();
        var discarded = CreateDeadLetter(database, "discard-target", TestDatabase.Now.AddHours(-2), 4);
        var retained = CreateDeadLetter(database, "retained-target", TestDatabase.Now.AddHours(-1), 2);
        await database.Save(discarded, retained);
        var before = EntryState.From(discarded);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDeadLetterStore<InboxEntry>>();

        var succeeded = await store.Discard(discarded.Key, CancellationToken.None);

        await Assert.That(succeeded).IsTrue();
        var entries = await database.ReadEntries();
        await Assert.That(entries.Length).IsEqualTo(2);
        await Assert.That(EntryState.From(entries.Single(entry => entry.Key == discarded.Key))).IsEqualTo(before with
        {
            DiscardedAt = TestDatabase.Now,
            ClaimId = null,
            ClaimedUntil = null,
            NextAttemptAt = null
        });
        await Assert.That(EntryState.From(entries.Single(entry => entry.Key == retained.Key))).IsEqualTo(EntryState.From(retained));
        var listed = await store.Get(10, CancellationToken.None);
        await Assert.That(listed.Count).IsEqualTo(1);
        await Assert.That(listed[0]).IsEqualTo(Project(retained));
    }

    [Test]
    [Arguments("Missing", true)]
    [Arguments("Missing", false)]
    [Arguments("Discarded", true)]
    [Arguments("Discarded", false)]
    [Arguments("Completed", true)]
    [Arguments("Completed", false)]
    [Property("TestCaseId", "OPS-001-AC-04")]
    public async Task Manage_IneligibleKey_ReturnsFalseAndPreservesAllTargets(string state, bool requeue)
    {
        await using var database = await server.CreateDatabase();
        var failed = CreateDeadLetter(database, "failed", TestDatabase.Now.AddHours(-2), 4);
        var discarded = CreateDeadLetter(database, "discarded", TestDatabase.Now.AddHours(-1), 2);
        discarded.DiscardedAt = TestDatabase.Now;
        var completed = CreateDeadLetter(database, "completed", TestDatabase.Now.AddHours(-3), 3);
        completed.DeadLetteredAt = null;
        completed.ProcessedAt = TestDatabase.Now.AddMinutes(-10);
        await database.Save(failed, discarded, completed);
        var before = (await database.ReadEntries()).Select(EntryState.From).ToArray();
        var key = state switch
        {
            "Discarded" => discarded.Key,
            "Completed" => completed.Key,
            _ => new EntryKey(failed.MessageId, "missing-target")
        };
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDeadLetterStore<InboxEntry>>();

        var succeeded = requeue
            ? await store.Requeue(key, CancellationToken.None)
            : await store.Discard(key, CancellationToken.None);

        await Assert.That(succeeded).IsFalse();
        await Assert.That((await database.ReadEntries()).Select(EntryState.From).ToArray()).IsEquivalentTo(before);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    [Property("TestCaseId", "OPS-001-AC-05")]
    public async Task Manage_AmbientTransaction_RejectsBoundaryWithoutMutatingTargets(bool requeue)
    {
        await using var database = await server.CreateDatabase();
        var entry = CreateDeadLetter(database, "failed", TestDatabase.Now.AddHours(-1), 2);
        await database.Save(entry);
        var before = EntryState.From((await database.ReadEntries()).Single());
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDeadLetterStore<InboxEntry>>();

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await Assert.That(async () =>
            {
                if (requeue)
                {
                    await store.Requeue(entry.Key, CancellationToken.None);
                }
                else
                {
                    await store.Discard(entry.Key, CancellationToken.None);
                }
            }).Throws<InvalidOperationException>();
        }

        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(before);
    }

    [Test]
    [Property("TestCaseId", "OPS-002-AC-01")]
    public async Task Requeue_FailedFanOutSibling_PreservesCompletedSiblingAndOriginalIdentities()
    {
        await using var database = await server.CreateDatabase();
        var eventId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var completed = database.CreateEntry("target-a", eventId, messageId);
        completed.Attempts = 2;
        completed.ProcessedAt = TestDatabase.Now.AddHours(-1);
        var failed = new InboxEntry
        {
            MessageId = messageId,
            EventId = eventId,
            TargetId = "target-b",
            Message = completed.Message,
            CorrelationId = completed.CorrelationId,
            CausationId = completed.CausationId,
            CreatedAt = completed.CreatedAt,
            Attempts = 4,
            DeadLetteredAt = TestDatabase.Now
        };
        await database.Save(completed, failed);
        var completedBefore = EntryState.From(completed);
        var failedBefore = EntryState.From(failed);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDeadLetterStore<InboxEntry>>();

        var succeeded = await store.Requeue(failed.Key, CancellationToken.None);

        await Assert.That(succeeded).IsTrue();
        var entries = await database.ReadEntries();
        await Assert.That(entries.Length).IsEqualTo(2);
        await Assert.That(EntryState.From(entries.Single(entry => entry.Key == completed.Key))).IsEqualTo(completedBefore);
        await Assert.That(EntryState.From(entries.Single(entry => entry.Key == failed.Key))).IsEqualTo(failedBefore with
        {
            Attempts = 0,
            DeadLetteredAt = null
        });
    }

    private static InboxEntry CreateDeadLetter(TestDatabase database, string targetId, DateTime deadLetteredAt, int attempts)
    {
        var entry = database.CreateEntry(targetId);
        entry.Attempts = attempts;
        entry.DeadLetteredAt = deadLetteredAt;
        entry.FailureCategory = FailureCategory.Permanent;
        entry.FailureCode = $"work.{targetId}.invalid";
        entry.ExceptionType = typeof(InvalidOperationException).FullName;
        entry.FailureMessage = $"Failed work for {targetId}.";
        entry.LastFailedAt = deadLetteredAt;
        entry.LastFailedAttempt = attempts;
        return entry;
    }

    private static DeadLetter Project(InboxEntry entry) => new(
        entry.Key, entry.EventId, entry.DeadLetteredAt!.Value, entry.Attempts, entry.FailureCode, entry.FailureMessage);
}

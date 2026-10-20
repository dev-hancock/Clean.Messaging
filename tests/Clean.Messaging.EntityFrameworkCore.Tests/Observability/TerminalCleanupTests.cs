using Clean.Messaging.Abstractions;
using Clean.Messaging.EntityFrameworkCore.Tests.Infrastructure;
using Clean.Messaging.Failures;
using Clean.Messaging.Inbox;
using Clean.Messaging.Inbox.Persistence;
using Clean.Messaging.Inbox.Runtime;
using Clean.Messaging.Persistence;
using Clean.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.EntityFrameworkCore.Tests.Observability;

[ClassDataSource<PostgreSqlServer>(Shared = SharedType.PerTestSession)]
public sealed class TerminalCleanupTests(PostgreSqlServer server)
{
    [Test]
    [Arguments("Completed")]
    [Arguments("DeadLettered")]
    [Arguments("Discarded")]
    [Property("TestCaseId", "OPS-003-AC-01")]
    public async Task Cleanup_TerminalTimestampsAroundCutoff_DeletesOnlyStrictlyOlderEntries(string outcome)
    {
        await using var database = await server.CreateDatabase();
        var cutoff = TestDatabase.Now.AddHours(-1);
        var older = CreateTerminalEntry(database, "older", outcome, cutoff.AddSeconds(-1));
        var equal = CreateTerminalEntry(database, "equal", outcome, cutoff);
        var newer = CreateTerminalEntry(database, "newer", outcome, cutoff.AddSeconds(1));
        await database.Save(newer, older, equal);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();

        var deleted = await store.Cleanup(cutoff, 10, CancellationToken.None);

        await Assert.That(deleted).IsEqualTo(1);
        await Assert.That((await database.ReadEntries()).Select(EntryState.From).ToArray()).IsEquivalentTo(new[]
        {
            EntryState.From(equal), EntryState.From(newer)
        });
    }

    [Test]
    [Arguments("Pending")]
    [Arguments("Retrying")]
    [Arguments("Claimed")]
    [Arguments("ExpiredClaim")]
    [Property("TestCaseId", "OPS-003-AC-02")]
    public async Task Cleanup_VeryOldUnresolvedEntry_PreservesWorkAndItsEntireState(string state)
    {
        await using var database = await server.CreateDatabase();
        var id = Guid.NewGuid();
        var unresolved = new InboxEntry
        {
            MessageId = id,
            EventId = Guid.NewGuid(),
            TargetId = "unresolved",
            Message = database.Services.GetRequiredService<IMessageSerializer>().Serialize(new WorkItem(id, "old unresolved work")),
            CreatedAt = TestDatabase.Now.AddYears(-10)
        };
        if (state == "Retrying")
        {
            unresolved.Attempts = 3;
            unresolved.NextAttemptAt = TestDatabase.Now.AddMinutes(10);
            unresolved.FailureCategory = FailureCategory.Transient;
            unresolved.ExceptionType = typeof(TimeoutException).FullName;
            unresolved.FailureCode = "work.temporarily_unavailable";
            unresolved.FailureMessage = "Retry after the dependency recovers.";
            unresolved.LastFailedAt = TestDatabase.Now.AddMinutes(-1);
            unresolved.LastFailedAttempt = 3;
        }
        else if (state is "Claimed" or "ExpiredClaim")
        {
            unresolved.Attempts = 1;
            unresolved.ClaimId = "worker";
            unresolved.ClaimedUntil = state == "Claimed"
                ? TestDatabase.Now.Add(TestDatabase.Lease)
                : TestDatabase.Now.Subtract(TestDatabase.Lease);
        }
        var completed = CreateTerminalEntry(database, "completed", "Completed", TestDatabase.Now.AddHours(-2));
        await database.Save(unresolved, completed);
        var before = EntryState.From(unresolved);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();

        var deleted = await store.Cleanup(TestDatabase.Now.AddHours(-1), 10, CancellationToken.None);

        await Assert.That(deleted).IsEqualTo(1);
        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(before);
    }

    [Test]
    [Arguments(1)]
    [Arguments(2)]
    [Property("TestCaseId", "OPS-003-AC-03")]
    public async Task Cleanup_MoreEligibleTargetsThanLimit_ReportsBoundedActualDeletionAndProtectsOtherWork(int limit)
    {
        await using var database = await server.CreateDatabase();
        var cutoff = TestDatabase.Now.AddHours(-1);
        var eligible = Enumerable.Range(0, 5)
            .Select(index => CreateTerminalEntry(database, $"eligible-{index}", index % 2 == 0 ? "Completed" : "DeadLettered",
                cutoff.AddMinutes(-index - 1)))
            .ToArray();
        var equal = CreateTerminalEntry(database, "equal", "Completed", cutoff);
        var newer = CreateTerminalEntry(database, "newer", "DeadLettered", cutoff.AddMinutes(1));
        var pending = database.CreateEntry("pending");
        var protectedEntries = new[] { equal, newer, pending };
        await database.Save([.. eligible, .. protectedEntries]);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();

        var deleted = await store.Cleanup(cutoff, limit, CancellationToken.None);

        var after = await database.ReadEntries();
        var eligibleKeys = eligible.Select(entry => entry.Key).ToHashSet();
        var retainedEligible = after.Where(entry => eligibleKeys.Contains(entry.Key)).ToArray();
        await Assert.That(deleted).IsGreaterThan(0);
        await Assert.That(deleted).IsLessThanOrEqualTo(limit);
        await Assert.That(deleted).IsEqualTo(eligible.Length - retainedEligible.Length);
        await Assert.That(after.Length).IsEqualTo(eligible.Length + protectedEntries.Length - deleted);
        await Assert.That(after.Where(entry => !eligibleKeys.Contains(entry.Key)).Select(EntryState.From).ToArray())
            .IsEquivalentTo(protectedEntries.Select(EntryState.From).ToArray());
        foreach (var entry in retainedEligible)
        {
            await Assert.That(EntryState.From(entry)).IsEqualTo(EntryState.From(eligible.Single(original => original.Key == entry.Key)));
        }
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    [Property("TestCaseId", "OPS-003-AC-05")]
    public async Task Accept_CleanedCompletedTarget_RetainsDuplicateProtectionWhileExplicitReplayRecreatesOriginalWork(bool newDeliveryId)
    {
        await using var database = await server.CreateDatabase(services => services.AddConsumer<WorkItem, NoOpConsumer>("work-consumer"));
        await using var scope = database.CreateScope();
        var inbox = scope.ServiceProvider.GetRequiredService<IInbox>();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var manager = scope.ServiceProvider.GetRequiredService<IInboxManager>();
        var messageId = Guid.NewGuid();
        var envelope = new InboxMessage<WorkItem>(messageId, new WorkItem(messageId, "retained admission"),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        await inbox.Accept(envelope);
        var original = EntryState.From((await database.ReadEntries()).Single());
        var owned = (await store.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        await Assert.That(await store.Complete(owned.Entry.Key, owned.ClaimId, TestDatabase.Now, CancellationToken.None)).IsTrue();
        await Assert.That(await store.Cleanup(TestDatabase.Now.AddSeconds(1), 10, CancellationToken.None)).IsEqualTo(1);
        await Assert.That((await database.ReadEntries()).Length).IsEqualTo(0);

        var duplicate = newDeliveryId ? envelope with { Id = Guid.NewGuid() } : envelope;
        await inbox.Accept(duplicate);

        await Assert.That((await database.ReadEntries()).Length).IsEqualTo(0);
        var replay = await manager.Replay<WorkItem>("work-consumer");
        await Assert.That(replay.Matched).IsEqualTo(1);
        await Assert.That(replay.Replayed).IsEqualTo(1);
        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(original);
    }

    private static InboxEntry CreateTerminalEntry(TestDatabase database, string targetId, string outcome, DateTime terminalAt)
    {
        var entry = database.CreateEntry(targetId);
        entry.Attempts = 2;
        switch (outcome)
        {
            case "Completed":
                entry.ProcessedAt = terminalAt;
                break;
            case "DeadLettered":
                entry.DeadLetteredAt = terminalAt;
                break;
            case "Discarded":
                // Coincident terminal times exercise cutoff without choosing the reserved discard-age policy.
                entry.DeadLetteredAt = terminalAt;
                entry.DiscardedAt = terminalAt;
                break;
        }
        return entry;
    }

    private sealed class NoOpConsumer : IMessageConsumer<WorkItem>
    {
        public NoOpConsumer() { }

        public ValueTask Handle(WorkItem message, CancellationToken cancellationToken) => ValueTask.CompletedTask;
    }
}

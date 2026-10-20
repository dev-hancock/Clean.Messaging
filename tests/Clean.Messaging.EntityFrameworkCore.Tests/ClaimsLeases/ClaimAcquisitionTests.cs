using Clean.Messaging.EntityFrameworkCore.Tests.Infrastructure;
using Clean.Messaging.Inbox.Persistence;
using Clean.Messaging.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.EntityFrameworkCore.Tests.ClaimsLeases;

[ClassDataSource<PostgreSqlServer>(Shared = SharedType.PerTestSession)]
public sealed class ClaimAcquisitionTests(PostgreSqlServer server)
{
    [Test]
    [Arguments(1)]
    [Arguments(3)]
    [Arguments(10)]
    public async Task Claim_PositiveBatchLimit_PersistsBoundedOwnershipWithStableIdentities(int limit)
    {
        await using var database = await server.CreateDatabase();
        var entries = Enumerable.Range(1, 4).Select(index => database.CreateEntry($"target-{index}")).ToArray();
        await database.Save(entries);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();

        var claimed = await store.Claim(TestDatabase.Now, limit, TestDatabase.Lease, CancellationToken.None);

        await Assert.That(claimed.Count).IsGreaterThan(0);
        await Assert.That(claimed.Count <= limit).IsTrue();
        var retained = await database.ReadEntries();
        await Assert.That(retained.Where(entry => entry.ClaimId != null).Select(entry => entry.Key).ToArray())
            .IsEquivalentTo(claimed.Select(owner => owner.Entry.Key).ToArray());
        foreach (var owner in claimed)
        {
            var stored = retained.Single(entry => entry.Key == owner.Entry.Key);
            var original = entries.Single(entry => entry.Key == owner.Entry.Key);
            await Assert.That(EntryState.From(stored)).IsEqualTo(EntryState.From(original) with
            {
                ClaimId = owner.ClaimId, ClaimedUntil = TestDatabase.Now.Add(TestDatabase.Lease), Attempts = stored.Attempts
            });
            await Assert.That(EntryState.From(owner.Entry)).IsEqualTo(EntryState.From(stored));
        }
        foreach (var unclaimed in retained.Where(entry => entry.ClaimId == null))
        {
            await Assert.That(EntryState.From(unclaimed)).IsEqualTo(EntryState.From(entries.Single(entry => entry.Key == unclaimed.Key)));
        }
    }

    [Test]
    [Property("TestCaseId", "CLAIM-001-AC-02")]
    public async Task Claim_TerminalAndUnfinishedTargets_AcquiresOnlyUnfinishedWork()
    {
        await using var database = await server.CreateDatabase();
        var pending = database.CreateEntry("pending");
        var completed = database.CreateEntry("completed");
        completed.ProcessedAt = TestDatabase.Now.AddMinutes(-5);
        var deadLettered = database.CreateEntry("dead-lettered");
        deadLettered.DeadLetteredAt = TestDatabase.Now.AddMinutes(-5);
        var discarded = database.CreateEntry("discarded");
        discarded.DiscardedAt = TestDatabase.Now.AddMinutes(-5);
        await database.Save(pending, completed, deadLettered, discarded);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();

        var claimed = await store.Claim(TestDatabase.Now, 10, TestDatabase.Lease, CancellationToken.None);

        await Assert.That(claimed.Select(item => item.Entry.Key).ToArray()).IsEquivalentTo(new[] { pending.Key });
        var retained = await database.ReadEntries();
        foreach (var terminal in new[] { completed, deadLettered, discarded })
        {
            await Assert.That(EntryState.From(retained.Single(entry => entry.Key == terminal.Key)))
                .IsEqualTo(EntryState.From(terminal));
        }
    }
}

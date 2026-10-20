using Clean.Messaging.EntityFrameworkCore.Tests.Infrastructure;
using Clean.Messaging.Inbox.Persistence;
using Clean.Messaging.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.EntityFrameworkCore.Tests.ClaimsLeases;

[ClassDataSource<PostgreSqlServer>(Shared = SharedType.PerTestSession)]
public sealed class ConcurrentOwnershipTests(PostgreSqlServer server)
{
    [Test]
    [Property("TestCaseId", "CLAIM-002-AC-01")]
    public async Task Complete_ConcurrentClaimants_OnlyRetainedOwnerSettlesTarget()
    {
        await using var database = await server.CreateDatabase();
        var entry = database.CreateEntry("billing");
        await database.Save(entry);
        await using var blocker = database.CreateContext();
        await using var transaction = await blocker.Database.BeginTransactionAsync();
        await blocker.Entries.Where(e => e.MessageId == entry.MessageId && e.TargetId == entry.TargetId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.Attempts, e => e.Attempts));
        await using var firstScope = database.CreateScope();
        await using var secondScope = database.CreateScope();
        var firstStore = firstScope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var secondStore = secondScope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var firstClaim = firstStore.Claim(TestDatabase.Now, 1, TestDatabase.Lease, cancellation.Token).AsTask();
        var secondClaim = secondStore.Claim(TestDatabase.Now, 1, TestDatabase.Lease, cancellation.Token).AsTask();
        IReadOnlyList<OwnedEntry<InboxEntry>>[] batches;
        try
        {
            await WaitForBlockedUpdates(database, 2, cancellation.Token);
            await transaction.CommitAsync(cancellation.Token);
            batches = await Task.WhenAll(firstClaim, secondClaim);
        }
        finally
        {
            cancellation.Cancel();
        }
        var owners = batches.SelectMany(batch => batch).ToArray();
        var current = (await database.ReadEntries()).Single();
        await Assert.That(owners.Length).IsGreaterThan(0);
        await Assert.That(owners.Any(owner => owner.ClaimId == current.ClaimId)).IsTrue();
        var successes = 0;

        foreach (var owner in owners)
        {
            await using var scope = database.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
            var completed = await store.Complete(owner.Entry.Key, owner.ClaimId, TestDatabase.Now, CancellationToken.None);
            await Assert.That(completed).IsEqualTo(owner.ClaimId == current.ClaimId);
            if (completed) successes++;
        }

        await Assert.That(successes).IsEqualTo(1);
        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(EntryState.From(current) with
        {
            ProcessedAt = TestDatabase.Now, ClaimId = null, ClaimedUntil = null
        });
    }

    [Test]
    [Property("TestCaseId", "CLAIM-001-AC-02")]
    public async Task Claim_BlockedWhileCurrentOwnerCompletes_DoesNotReacquireTerminalTarget()
    {
        await using var database = await server.CreateDatabase();
        var entry = database.CreateEntry("billing");
        await database.Save(entry);
        await using var currentScope = database.CreateScope();
        var currentStore = currentScope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var owner = (await currentStore.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        await using var completingContext = currentScope.ServiceProvider.GetRequiredService<MessagingDbContext>();
        await using var transaction = await completingContext.Database.BeginTransactionAsync();
        await Assert.That(await currentStore.Complete(entry.Key, owner.ClaimId, TestDatabase.Now, CancellationToken.None)).IsTrue();
        await using var successorScope = database.CreateScope();
        var successorStore = successorScope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var claiming = successorStore.Claim(TestDatabase.Now.AddMinutes(5), 1, TestDatabase.Lease, cancellation.Token).AsTask();
        IReadOnlyList<OwnedEntry<InboxEntry>> acquired;

        try
        {
            await WaitForBlockedUpdates(database, 1, cancellation.Token);
            await transaction.CommitAsync(cancellation.Token);
            acquired = await claiming;
        }
        finally
        {
            cancellation.Cancel();
        }

        await Assert.That(acquired.Count).IsEqualTo(0);
        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(EntryState.From(owner.Entry) with
        {
            ProcessedAt = TestDatabase.Now, ClaimId = null, ClaimedUntil = null
        });
    }

    [Test]
    public async Task Complete_CancelledWhileWaitingForDatabaseLock_PropagatesCancellationWithoutSettlement()
    {
        await using var database = await server.CreateDatabase();
        var entry = database.CreateEntry("billing");
        await database.Save(entry);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var owner = (await store.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        await using var blocker = database.CreateContext();
        await using var transaction = await blocker.Database.BeginTransactionAsync();
        await blocker.Entries.Where(e => e.MessageId == entry.MessageId && e.TargetId == entry.TargetId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.Attempts, e => e.Attempts));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
        var completing = store.Complete(entry.Key, owner.ClaimId, TestDatabase.Now, cancellation.Token).AsTask();

        try
        {
            await WaitForBlockedUpdates(database, 1, timeout.Token);
            cancellation.Cancel();
            await Assert.That(async () => await completing).Throws<OperationCanceledException>();
        }
        finally
        {
            cancellation.Cancel();
        }

        await Assert.That(EntryState.From((await database.ReadEntries()).Single())).IsEqualTo(EntryState.From(owner.Entry));
    }

    private static async Task WaitForBlockedUpdates(TestDatabase database, int count, CancellationToken cancellationToken)
    {
        await using var context = database.CreateContext();
        await context.Database.OpenConnectionAsync(cancellationToken);
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT count(*) FROM pg_stat_activity
            WHERE datname = current_database()
              AND wait_event_type = 'Lock'
              AND query LIKE 'UPDATE %'
            """;
        while (Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) < count)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
        }
    }
}

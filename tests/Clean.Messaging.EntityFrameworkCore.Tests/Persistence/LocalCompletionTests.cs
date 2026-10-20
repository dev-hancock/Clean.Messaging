using Clean.Messaging.Abstractions;
using Clean.Messaging.EntityFrameworkCore.Tests.Infrastructure;
using Clean.Messaging.Inbox.Dispatching;
using Clean.Messaging.Inbox.Persistence;
using Clean.Messaging.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Clean.Messaging.EntityFrameworkCore.Tests.Persistence;

[ClassDataSource<PostgreSqlServer>(Shared = SharedType.PerTestSession)]
public sealed class LocalCompletionTests(PostgreSqlServer server)
{
    private const string ConsumerId = "local-effects";

    [Test]
    [Property("TestCaseId", "PERSIST-002-AC-01")]
    public async Task Dispatch_LocalConsumerSucceeds_CommitsEffectAndCompletionTogether()
    {
        await using var database = await server.CreateDatabase(services => services.AddConsumer<WorkItem, SavingConsumer>(ConsumerId));
        var entry = database.CreateEntry(ConsumerId);
        await database.Save(entry);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var owned = (await store.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IInboxDispatcher>();

        var completed = await dispatcher.Dispatch(owned, CancellationToken.None);

        await Assert.That(completed).IsTrue();
        await using var reader = database.CreateContext();
        await Assert.That(await reader.Effects.AsNoTracking().SingleAsync()).IsEqualTo(new LocalEffect(entry.MessageId, "durable work"));
        var retained = await reader.Entries.AsNoTracking().SingleAsync();
        await Assert.That(EntryState.From(retained)).IsEqualTo(EntryState.From(owned.Entry) with
        {
            ProcessedAt = TestDatabase.Now, ClaimId = null, ClaimedUntil = null
        });
    }

    [Test]
    [Property("TestCaseId", "PERSIST-002-AC-02")]
    public async Task Dispatch_ConsumerFailsAfterDatabaseWrite_RollsBackEffectWithoutCompletingTarget()
    {
        await using var database = await server.CreateDatabase(services => services.AddConsumer<WorkItem, FailingConsumer>(ConsumerId));
        var entry = database.CreateEntry(ConsumerId);
        await database.Save(entry);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var owned = (await store.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IInboxDispatcher>();

        await Assert.That(async () => await dispatcher.Dispatch(owned, CancellationToken.None)).Throws<InvalidOperationException>();

        await using var reader = database.CreateContext();
        await Assert.That(await reader.Effects.CountAsync()).IsEqualTo(0);
        await Assert.That(EntryState.From(await reader.Entries.AsNoTracking().SingleAsync())).IsEqualTo(EntryState.From(owned.Entry));
    }

    [Test]
    [Property("TestCaseId", "PERSIST-002-AC-03")]
    public async Task Dispatch_TargetReclaimedBySuccessor_RollsBackObsoleteConsumersChanges()
    {
        var later = TestDatabase.Now.Add(TestDatabase.Lease).AddSeconds(1);
        await using var database = await server.CreateDatabase(services =>
        {
            services.AddSingleton<TimeProvider>(new AtTime(later));
            services.AddConsumer<WorkItem, SavingConsumer>(ConsumerId);
        });
        var entry = database.CreateEntry(ConsumerId);
        await database.Save(entry);
        await using var firstScope = database.CreateScope();
        var firstStore = firstScope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var first = (await firstStore.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        await using var successorScope = database.CreateScope();
        var successorStore = successorScope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var successor = (await successorStore.Claim(later, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        var dispatcher = firstScope.ServiceProvider.GetRequiredService<IInboxDispatcher>();

        var completed = await dispatcher.Dispatch(first, CancellationToken.None);

        await Assert.That(completed).IsFalse();
        await using var reader = database.CreateContext();
        await Assert.That(await reader.Effects.CountAsync()).IsEqualTo(0);
        await Assert.That(EntryState.From(await reader.Entries.AsNoTracking().SingleAsync())).IsEqualTo(EntryState.From(successor.Entry));
    }

    [Test]
    [Property("TestCaseId", "PERSIST-002-AC-04")]
    public async Task Dispatch_LocalSaveFailsAfterIndependentCommit_PreservesExternalEffectWithoutLocalCompletion()
    {
        await using var externalDatabase = await server.CreateDatabase();
        await using var database = await server.CreateDatabase(services =>
        {
            services.AddSingleton(new ExternalEffectWriter(externalDatabase));
            services.AddConsumer<WorkItem, ExternalThenConflictingConsumer>(ConsumerId);
        });
        var entry = database.CreateEntry(ConsumerId);
        var original = new LocalEffect(entry.MessageId, "existing local effect");
        await database.Save(entry);
        await using (var setup = database.CreateContext())
        {
            setup.Effects.Add(original);
            await setup.SaveChangesAsync();
        }
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDurableStore<InboxEntry>>();
        var owned = (await store.Claim(TestDatabase.Now, 1, TestDatabase.Lease, CancellationToken.None)).Single();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IInboxDispatcher>();

        var error = await Assert.That(async () => await dispatcher.Dispatch(owned, CancellationToken.None)).Throws<DbUpdateException>();

        await Assert.That(error!.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }).IsTrue();
        await using var localReader = database.CreateContext();
        await Assert.That(await localReader.Effects.AsNoTracking().SingleAsync()).IsEqualTo(original);
        await Assert.That(EntryState.From(await localReader.Entries.AsNoTracking().SingleAsync())).IsEqualTo(EntryState.From(owned.Entry));
        await using var externalReader = externalDatabase.CreateContext();
        await Assert.That(await externalReader.Effects.AsNoTracking().SingleAsync()).IsEqualTo(new LocalEffect(entry.MessageId, "durable work"));
    }

    private sealed class SavingConsumer(MessagingDbContext db) : IMessageConsumer<WorkItem>
    {
        public async ValueTask Handle(WorkItem message, CancellationToken cancellationToken)
        {
            db.Effects.Add(new LocalEffect(message.Id, message.Value));
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private sealed class FailingConsumer(MessagingDbContext db) : IMessageConsumer<WorkItem>
    {
        public async ValueTask Handle(WorkItem message, CancellationToken cancellationToken)
        {
            db.Effects.Add(new LocalEffect(message.Id, message.Value));
            await db.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("The consumer failed after writing participating state.");
        }
    }

    private sealed class ExternalThenConflictingConsumer(MessagingDbContext db, ExternalEffectWriter external) : IMessageConsumer<WorkItem>
    {
        public async ValueTask Handle(WorkItem message, CancellationToken cancellationToken)
        {
            await external.Write(message, cancellationToken);
            db.Effects.Add(new LocalEffect(message.Id, message.Value));
        }
    }

    private sealed class ExternalEffectWriter(TestDatabase database)
    {
        public async Task Write(WorkItem message, CancellationToken cancellationToken)
        {
            await using var context = database.CreateContext();
            context.Effects.Add(new LocalEffect(message.Id, message.Value));
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private sealed class AtTime(DateTime now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(now);
    }
}

using Clean.Messaging.EntityFrameworkCore.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Clean.Messaging.EntityFrameworkCore.Tests.Persistence;

[ClassDataSource<PostgreSqlServer>(Shared = SharedType.PerTestSession)]
public sealed class RetainedIdentityTests(PostgreSqlServer server)
{
    [Test]
    [Property("TestCaseId", "PERSIST-001-AC-01")]
    public async Task SaveChanges_DuplicateLogicalTarget_RejectsSecondDelivery()
    {
        await using var database = await server.CreateDatabase();
        var first = database.CreateEntry("billing");
        var duplicate = database.CreateEntry(first.TargetId, first.EventId);
        await database.Save(first);
        await using var writer = database.CreateContext();
        writer.Entries.Add(duplicate);

        var error = await Assert.That(() => writer.SaveChangesAsync()).Throws<DbUpdateException>();

        await Assert.That(error!.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }).IsTrue();
        var retained = await database.ReadEntries();
        await Assert.That(retained.Select(EntryState.From).ToArray()).IsEquivalentTo(new[] { EntryState.From(first) });
    }

    [Test]
    [Property("TestCaseId", "PERSIST-001-AC-02")]
    public async Task SaveChanges_SharedEventWithDistinctTargets_PersistsBothResponsibilities()
    {
        await using var database = await server.CreateDatabase();
        var first = database.CreateEntry("billing");
        var sibling = database.CreateEntry("audit", first.EventId, first.MessageId);

        await database.Save(first, sibling);

        var retained = await database.ReadEntries();
        await Assert.That(retained.Select(EntryState.From).ToArray())
            .IsEquivalentTo(new[] { EntryState.From(first), EntryState.From(sibling) });
    }

    [Test]
    [Property("TestCaseId", "PERSIST-001-AC-03")]
    public async Task SaveChanges_DuplicateDeliveryKey_RejectsConflictingRecord()
    {
        await using var database = await server.CreateDatabase();
        var first = database.CreateEntry("billing");
        var duplicate = database.CreateEntry(first.TargetId, messageId: first.MessageId);
        await database.Save(first);
        await using var writer = database.CreateContext();
        writer.Entries.Add(duplicate);

        var error = await Assert.That(() => writer.SaveChangesAsync()).Throws<DbUpdateException>();

        await Assert.That(error!.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }).IsTrue();
        var retained = await database.ReadEntries();
        await Assert.That(retained.Select(EntryState.From).ToArray()).IsEquivalentTo(new[] { EntryState.From(first) });
    }
}

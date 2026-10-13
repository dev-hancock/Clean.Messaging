using Clean.Messaging.Outbox;
using MessagingOutbox = Clean.Messaging.Outbox.Outbox;
using Clean.Messaging.Outbox.Persistence;
using Clean.Messaging.Outbox.Transport;
using Clean.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Clean.Messaging.Tests.Outbox;

public sealed class OutboxIdentityTests
{
    [Test]
    [Property("TestCaseId", "OUTBOX-002-AC-01")]
    [Arguments(false, false)]
    [Arguments(true, false)]
    [Arguments(false, true)]
    [Arguments(true, true)]
    public async Task Create_AmbientOrExplicitMetadata_PreservesOverridesAndImmediateCausation(
        bool explicitCorrelation, bool explicitCausation)
    {
        using var provider = CreateServices().BuildServiceProvider();
        var context = new MessageContext();
        var deliveryId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var parentCausation = Guid.NewGuid();
        var overrideCorrelation = explicitCorrelation ? Guid.NewGuid() : (Guid?)null;
        var overrideCausation = explicitCausation ? Guid.NewGuid() : (Guid?)null;
        using var active = context.Push(deliveryId, correlationId, parentCausation, "consumer");
        var targets = new Targets();
        var factory = new OutboxEntryFactory([targets], provider.GetRequiredService<IMessageSerializer>(), context, TimeProvider.System);

        var batch = factory.Create(new OutboxMessage<Message>(Guid.NewGuid(), new Message("payload"),
            overrideCorrelation, overrideCausation));

        await Assert.That(batch.Entries.Count).IsEqualTo(2);
        foreach (var entry in batch.Entries)
        {
            await Assert.That(entry.CorrelationId).IsEqualTo(overrideCorrelation ?? correlationId);
            await Assert.That(entry.CausationId).IsEqualTo(overrideCausation ?? deliveryId);
        }
    }

    [Test]
    [Property("TestCaseId", "OUTBOX-002-AC-01")]
    public async Task Create_NoAmbientMetadata_LeavesCorrelationAndCausationAbsent()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var factory = new OutboxEntryFactory([new Targets()], provider.GetRequiredService<IMessageSerializer>(),
            new MessageContext(), TimeProvider.System);

        var batch = factory.Create(new OutboxMessage<Message>(Guid.NewGuid(), new Message("payload")));

        await Assert.That(batch.Entries.Count).IsEqualTo(2);
        foreach (var entry in batch.Entries)
        {
            await Assert.That(entry.CorrelationId).IsNull();
            await Assert.That(entry.CausationId).IsNull();
        }
    }

    [Test]
    [Property("TestCaseId", "OUTBOX-001-AC-02")]
    public async Task Enqueue_MultipleTargets_StagesDistinctDeliveryAndLogicalEventIdentity()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var serializer = provider.GetRequiredService<IMessageSerializer>();
        var factory = new OutboxEntryFactory([new Targets()], serializer, new MessageContext(), TimeProvider.System);
        var staged = new List<OutboxBatch>();
        var store = RecordStaging(staged);
        var outbox = new MessagingOutbox(factory, store);
        var eventId = Guid.NewGuid();
        var message = new Message("payload");

        outbox.Enqueue(new OutboxMessage<Message>(eventId, message));
        outbox.Enqueue(new OutboxMessage<Message>(eventId, message));

        await Assert.That(staged.Count).IsEqualTo(2);
        await Assert.That(staged[0].MessageId).IsNotEqualTo(Guid.Empty);
        await Assert.That(staged[1].MessageId).IsNotEqualTo(Guid.Empty);
        await Assert.That(staged[0].MessageId).IsNotEqualTo(eventId);
        await Assert.That(staged[1].MessageId).IsNotEqualTo(eventId);
        await Assert.That(staged[0].MessageId).IsNotEqualTo(staged[1].MessageId);
        foreach (var batch in staged)
        {
            await Assert.That(batch.EventId).IsEqualTo(eventId);
            await Assert.That(batch.Entries.Select(x => x.TargetId).ToArray()).IsEquivalentTo(new[] { "first", "second" });
            foreach (var entry in batch.Entries)
            {
                await Assert.That(entry.MessageId).IsEqualTo(batch.MessageId);
                await Assert.That(entry.EventId).IsEqualTo(eventId);
            }
        }
    }

    [Test]
    [Property("TestCaseId", "OUTBOX-001-AC-02")]
    public async Task Enqueue_ConcreteMessage_StagesRegisteredPayloadAndCreationTime()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var serializer = provider.GetRequiredService<IMessageSerializer>();
        var now = new DateTimeOffset(2026, 1, 15, 12, 30, 0, TimeSpan.Zero);
        var factory = new OutboxEntryFactory([new Targets()], serializer, new MessageContext(), new FixedClock(now));
        var staged = new List<OutboxBatch>();
        var outbox = new MessagingOutbox(factory, RecordStaging(staged));
        var message = new Message("payload");

        outbox.Enqueue(new OutboxMessage<Message>(Guid.NewGuid(), message));

        await Assert.That(staged.Count).IsEqualTo(1);
        await Assert.That(staged[0].Entries.Count).IsEqualTo(2);
        foreach (var entry in staged[0].Entries)
        {
            await Assert.That(entry.Message.Type).IsEqualTo("message.v1");
            await Assert.That(serializer.Deserialize(entry.Message)).IsEqualTo(message);
            await Assert.That(entry.CreatedAt).IsEqualTo(now.UtcDateTime);
        }
    }

    [Test]
    [Property("TestCaseId", "IDENTITY-002-AC-01")]
    public async Task Enqueue_SharedCorrelation_StagesBothDistinctOperations()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var context = new MessageContext();
        var correlationId = Guid.NewGuid();
        using var active = context.Push(Guid.NewGuid(), correlationId);
        var factory = new OutboxEntryFactory([new Targets()], provider.GetRequiredService<IMessageSerializer>(),
            context, TimeProvider.System);
        var staged = new List<OutboxBatch>();
        var store = RecordStaging(staged);
        var outbox = new MessagingOutbox(factory, store);
        var firstEvent = Guid.NewGuid();
        var secondEvent = Guid.NewGuid();

        outbox.Enqueue(new OutboxMessage<Message>(firstEvent, new Message("first")));
        outbox.Enqueue(new OutboxMessage<Message>(secondEvent, new Message("second")));

        await Assert.That(staged.Select(x => x.EventId).ToArray()).IsEquivalentTo(new[] { firstEvent, secondEvent });
        await Assert.That(staged.SelectMany(x => x.Entries).Count()).IsEqualTo(4);
        foreach (var entry in staged.SelectMany(x => x.Entries))
        {
            await Assert.That(entry.CorrelationId).IsEqualTo((Guid?)correlationId);
        }
    }

    private static IServiceCollection CreateServices()
        => new ServiceCollection().AddContract<Message>("message.v1");

    private static IOutboxStore RecordStaging(List<OutboxBatch> staged)
    {
        var store = Substitute.For<IOutboxStore>();
        store.When(x => x.Stage(Arg.Any<OutboxBatch>())).Do(x => staged.Add(x.Arg<OutboxBatch>()));
        return store;
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed record Message(string Value);
    private sealed class Targets : IOutboxTargetProvider
    {
        public IEnumerable<OutboxTarget> GetTargets(Type messageType, object message)
            => [new("first", "local", null), new("second", "external", "destination")];
    }
}

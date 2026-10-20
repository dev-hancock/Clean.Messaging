using Clean.Messaging.Abstractions;
using Clean.Messaging.Processing;
using Clean.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Tests.Identity;

public sealed class MessageContextTests
{
    [Test]
    [Property("TestCaseId", "IDENTITY-003-AC-01")]
    public async Task Read_NoActiveScope_ExposesNoMetadata()
    {
        var context = new MessageContext();

        await Assert.That(context.MessageId).IsNull();
        await Assert.That(context.CorrelationId).IsNull();
        await Assert.That(context.CausationId).IsNull();
        await Assert.That(context.ConsumerId).IsNull();
    }

    [Test]
    [Property("TestCaseId", "IDENTITY-003-AC-02")]
    public async Task Push_NestedScopes_RestoresAllParentMetadata()
    {
        var context = new MessageContext();
        var messageId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        var innerMessageId = Guid.NewGuid();
        var innerCorrelationId = Guid.NewGuid();
        var innerCausationId = Guid.NewGuid();
        using (context.Push(messageId, correlationId, causationId, "parent"))
        {
            using (context.Push(innerMessageId, innerCorrelationId, innerCausationId, "child"))
            {
                await Task.Yield();
                await Assert.That(context.MessageId).IsEqualTo((Guid?)innerMessageId);
                await Assert.That(context.CorrelationId).IsEqualTo((Guid?)innerCorrelationId);
                await Assert.That(context.CausationId).IsEqualTo((Guid?)innerCausationId);
                await Assert.That(context.ConsumerId).IsEqualTo("child");
            }
            await Assert.That(context.MessageId).IsEqualTo((Guid?)messageId);
            await Assert.That(context.CorrelationId).IsEqualTo((Guid?)correlationId);
            await Assert.That(context.CausationId).IsEqualTo((Guid?)causationId);
            await Assert.That(context.ConsumerId).IsEqualTo("parent");
        }
        await Assert.That(context.MessageId).IsNull();
        await Assert.That(context.CorrelationId).IsNull();
        await Assert.That(context.CausationId).IsNull();
        await Assert.That(context.ConsumerId).IsNull();
    }

    [Test]
    [Property("TestCaseId", "IDENTITY-003-AC-03")]
    public async Task Dispose_RepeatedDisposal_DoesNotChangeCurrentScope()
    {
        var context = new MessageContext();
        var disposed = context.Push(Guid.NewGuid(), Guid.NewGuid());
        disposed.Dispose();
        var currentId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        using var current = context.Push(currentId, correlationId, causationId, "current");

        disposed.Dispose();

        await Assert.That(context.MessageId).IsEqualTo((Guid?)currentId);
        await Assert.That(context.CorrelationId).IsEqualTo((Guid?)correlationId);
        await Assert.That(context.CausationId).IsEqualTo((Guid?)causationId);
        await Assert.That(context.ConsumerId).IsEqualTo("current");
    }

    [Test]
    [Property("TestCaseId", "IDENTITY-003-AC-05")]
    public async Task Push_OptionalMetadata_DoesNotInventIdentifiers()
    {
        var context = new MessageContext();
        var messageId = Guid.NewGuid();
        using var active = context.Push(messageId, null);

        await Assert.That(context.MessageId).IsEqualTo((Guid?)messageId);
        await Assert.That(context.CorrelationId).IsNull();
        await Assert.That(context.CausationId).IsNull();
        await Assert.That(context.ConsumerId).IsNull();
    }

    [Test]
    [Property("TestCaseId", "IDENTITY-003-AC-04")]
    public async Task Push_OverlappingAsyncFlows_IsolatesMetadataAndRestoresInheritedParent()
    {
        var context = new MessageContext();
        var parentId = Guid.NewGuid();
        var parentCorrelation = Guid.NewGuid();
        var parentCausation = Guid.NewGuid();
        using var parent = context.Push(parentId, parentCorrelation, parentCausation, "parent");
        var firstReady = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondReady = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var firstCorrelation = Guid.NewGuid();
        var secondCorrelation = Guid.NewGuid();
        var firstCausation = Guid.NewGuid();
        var secondCausation = Guid.NewGuid();

        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<(Metadata Inherited, Metadata BeforeAwait, Metadata AfterAwait, Metadata Restored)> RunFlow(
            Guid id, Guid correlation, Guid causation, string consumer, TaskCompletionSource ready)
        {
            var inherited = ReadMetadata(context);
            Metadata beforeAwait;
            Metadata afterAwait;
            using (context.Push(id, correlation, causation, consumer))
            {
                beforeAwait = ReadMetadata(context);
                ready.SetResult();
                await release.Task;
                afterAwait = ReadMetadata(context);
            }
            return (inherited, beforeAwait, afterAwait, ReadMetadata(context));
        }

        var first = RunFlow(firstId, firstCorrelation, firstCausation, "first", firstReady);
        var second = RunFlow(secondId, secondCorrelation, secondCausation, "second", secondReady);
        var flows = Task.WhenAll(first, second);
        try
        {
            await Task.WhenAll(firstReady.Task, secondReady.Task).WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            release.TrySetResult();
            await flows.WaitAsync(TimeSpan.FromSeconds(10));
        }
        var observations = await flows;
        var parentMetadata = new Metadata(parentId, parentCorrelation, parentCausation, "parent");
        var firstMetadata = new Metadata(firstId, firstCorrelation, firstCausation, "first");
        var secondMetadata = new Metadata(secondId, secondCorrelation, secondCausation, "second");

        await Assert.That(observations[0].Inherited).IsEqualTo(parentMetadata);
        await Assert.That(observations[0].BeforeAwait).IsEqualTo(firstMetadata);
        await Assert.That(observations[0].AfterAwait).IsEqualTo(firstMetadata);
        await Assert.That(observations[0].Restored).IsEqualTo(parentMetadata);
        await Assert.That(observations[1].Inherited).IsEqualTo(parentMetadata);
        await Assert.That(observations[1].BeforeAwait).IsEqualTo(secondMetadata);
        await Assert.That(observations[1].AfterAwait).IsEqualTo(secondMetadata);
        await Assert.That(observations[1].Restored).IsEqualTo(parentMetadata);
        await Assert.That(ReadMetadata(context)).IsEqualTo(parentMetadata);
    }

    [Test]
    [Property("TestCaseId", "IDENTITY-003-AC-06")]
    public async Task Dispose_FailedHandlerInOwnedScope_RestoresParentMetadata()
    {
        var services = new ServiceCollection();
        services.AddMessaging().AddContract<Message>("context.v1").AddConsumer<Message, FailingConsumer>("child");
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = provider.GetRequiredService<MessageContext>();
        var consumer = scope.ServiceProvider.GetRequiredService<FailingConsumer>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<ConsumerDispatcher>();
        var data = provider.GetRequiredService<IMessageSerializer>().Serialize(new Message("payload"));
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var parentMetadata = new Metadata(parentId, Guid.NewGuid(), Guid.NewGuid(), "parent");
        var childMetadata = new Metadata(childId, Guid.NewGuid(), Guid.NewGuid(), "child");
        using var parent = context.Push(parentId, parentMetadata.CorrelationId,
            parentMetadata.CausationId, parentMetadata.ConsumerId);
        Metadata? restoredAfterFailure = null;

        await Assert.That(async () =>
        {
            try
            {
                using var child = context.Push(childId, childMetadata.CorrelationId,
                    childMetadata.CausationId, childMetadata.ConsumerId);
                await dispatcher.Dispatch("child", data, default);
            }
            finally
            {
                // Observe inside the failing flow: returning from an async delegate also restores AsyncLocal state.
                restoredAfterFailure = ReadMetadata(context);
            }
        }).Throws<InvalidOperationException>();

        await Assert.That(consumer.ObservedMetadata).IsEqualTo(childMetadata);
        await Assert.That(restoredAfterFailure).IsEqualTo(parentMetadata);
        await Assert.That(ReadMetadata(context)).IsEqualTo(parentMetadata);
    }

    private static Metadata ReadMetadata(IMessageContext context) =>
        new(context.MessageId, context.CorrelationId, context.CausationId, context.ConsumerId);

    private sealed record Metadata(Guid? MessageId, Guid? CorrelationId, Guid? CausationId, string? ConsumerId);
    private sealed record Message(string Value);

    private sealed class FailingConsumer(IMessageContext context) : IMessageConsumer<Message>
    {
        public Metadata? ObservedMetadata { get; private set; }

        public async ValueTask Handle(Message message, CancellationToken cancellationToken)
        {
            await Task.Yield();
            ObservedMetadata = ReadMetadata(context);
            throw new InvalidOperationException("controlled handler failure");
        }
    }
}

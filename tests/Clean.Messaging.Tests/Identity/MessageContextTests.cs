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
        using var current = context.Push(currentId, null);

        disposed.Dispose();

        await Assert.That(context.MessageId).IsEqualTo((Guid?)currentId);
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

        async Task RunFlow(Guid id, Guid correlation, Guid causation, string consumer,
            TaskCompletionSource ready, TaskCompletionSource otherReady)
        {
            try
            {
                await Assert.That(context.MessageId).IsEqualTo((Guid?)parentId);
                using (context.Push(id, correlation, causation, consumer))
                {
                    await Assert.That(context.MessageId).IsEqualTo((Guid?)id);
                    await Assert.That(context.CorrelationId).IsEqualTo((Guid?)correlation);
                    await Assert.That(context.CausationId).IsEqualTo((Guid?)causation);
                    await Assert.That(context.ConsumerId).IsEqualTo(consumer);
                    ready.TrySetResult();
                    await otherReady.Task.WaitAsync(TimeSpan.FromSeconds(10));
                    await Assert.That(context.MessageId).IsEqualTo((Guid?)id);
                    await Assert.That(context.CorrelationId).IsEqualTo((Guid?)correlation);
                    await Assert.That(context.CausationId).IsEqualTo((Guid?)causation);
                    await Assert.That(context.ConsumerId).IsEqualTo(consumer);
                }
                await Assert.That(context.MessageId).IsEqualTo((Guid?)parentId);
                await Assert.That(context.CorrelationId).IsEqualTo((Guid?)parentCorrelation);
                await Assert.That(context.CausationId).IsEqualTo((Guid?)parentCausation);
                await Assert.That(context.ConsumerId).IsEqualTo("parent");
            }
            finally
            {
                ready.TrySetResult();
            }
        }

        await Task.WhenAll(
            RunFlow(firstId, firstCorrelation, firstCausation, "first", firstReady, secondReady),
            RunFlow(secondId, secondCorrelation, secondCausation, "second", secondReady, firstReady));

        await Assert.That(context.MessageId).IsEqualTo((Guid?)parentId);
        await Assert.That(context.ConsumerId).IsEqualTo("parent");
    }

    [Test]
    [Property("TestCaseId", "IDENTITY-003-AC-06")]
    public async Task Dispose_ExceptionalScopeExit_RestoresParentMetadata()
    {
        var context = new MessageContext();
        var parentId = Guid.NewGuid();
        using var parent = context.Push(parentId, null, consumerId: "parent");

        await Assert.That(() =>
        {
            using var child = context.Push(Guid.NewGuid(), Guid.NewGuid(), consumerId: "child");
            throw new InvalidOperationException("controlled scope failure");
        }).Throws<InvalidOperationException>();

        await Assert.That(context.MessageId).IsEqualTo((Guid?)parentId);
        await Assert.That(context.CorrelationId).IsNull();
        await Assert.That(context.CausationId).IsNull();
        await Assert.That(context.ConsumerId).IsEqualTo("parent");
    }
}

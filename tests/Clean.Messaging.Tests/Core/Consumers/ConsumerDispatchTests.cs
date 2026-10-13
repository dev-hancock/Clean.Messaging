using Clean.Messaging.Abstractions;
using Clean.Messaging.Exceptions;
using Clean.Messaging.Processing;
using Clean.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Tests.Core.Consumers;

public sealed class ConsumerDispatchTests
{
    [Test]
    [Property("TestCaseId", "CORE-003-AC-04")]
    public async Task Dispatch_AsyncConsumer_WaitsForCompletionAndForwardsMessageAndToken()
    {
        var consumer = new AwaitingConsumer();
        using var provider = CreateServices()
            .AddConsumer<Message, AwaitingConsumer>("consumer")
            .AddSingleton(consumer)
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        using var cancellation = new CancellationTokenSource();
        var dispatcher = scope.ServiceProvider.GetRequiredService<ConsumerDispatcher>();
        var message = new Message("payload");
        var data = provider.GetRequiredService<IMessageSerializer>().Serialize(message);
        var dispatch = dispatcher.Dispatch("consumer", data, cancellation.Token).AsTask();
        var assertionsPassed = false;

        try
        {
            await consumer.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await Assert.That(dispatch.IsCompleted).IsFalse();
            await Assert.That(consumer.Message).IsEqualTo(message);
            await Assert.That(consumer.Token).IsEqualTo(cancellation.Token);
            assertionsPassed = true;
        }
        finally
        {
            consumer.Release.TrySetResult();
            try
            {
                await dispatch.WaitAsync(TimeSpan.FromSeconds(10));
            }
            catch (Exception cleanupFailure) when (!assertionsPassed)
            {
                Console.Error.WriteLine($"Dispatch cleanup also failed: {cleanupFailure}");
            }
        }
    }

    [Test]
    [Property("TestCaseId", "CORE-003-AC-04")]
    public async Task Dispatch_ConsumerFailure_PropagatesOriginalException()
    {
        var cause = new InvalidOperationException("controlled consumer failure");
        var consumer = new FailingConsumer(cause);
        using var provider = CreateServices()
            .AddConsumer<Message, FailingConsumer>("consumer")
            .AddSingleton(consumer)
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<ConsumerDispatcher>();
        var data = provider.GetRequiredService<IMessageSerializer>().Serialize(new Message("payload"));

        var exception = await Assert.That(async () =>
            await dispatcher.Dispatch("consumer", data, default))
            .Throws<InvalidOperationException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(ReferenceEquals(exception, cause)).IsTrue();
    }

    [Test]
    [Property("TestCaseId", "CORE-003-AC-05")]
    public async Task Dispatch_CancellationDuringHandling_PropagatesCancellationAndOriginalToken()
    {
        using var cancellation = new CancellationTokenSource();
        var consumer = new AwaitingConsumer();
        using var provider = CreateServices()
            .AddConsumer<Message, AwaitingConsumer>("consumer")
            .AddSingleton(consumer)
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<ConsumerDispatcher>();
        var data = provider.GetRequiredService<IMessageSerializer>().Serialize(new Message("payload"));
        var dispatch = dispatcher.Dispatch("consumer", data, cancellation.Token).AsTask();
        var assertionsPassed = false;

        try
        {
            await consumer.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await Assert.That(dispatch.IsCompleted).IsFalse();
            cancellation.Cancel();

            var exception = await Assert.That(() => dispatch.WaitAsync(TimeSpan.FromSeconds(10)))
                .Throws<OperationCanceledException>();
            await Assert.That(exception).IsNotNull();
            await Assert.That(exception.CancellationToken).IsEqualTo(cancellation.Token);
            await Assert.That(consumer.Token).IsEqualTo(cancellation.Token);
            assertionsPassed = true;
        }
        finally
        {
            consumer.Release.TrySetResult();
            try
            {
                cancellation.Cancel();
                await dispatch.WaitAsync(TimeSpan.FromSeconds(10));
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                // Cancellation is the asserted completion; observe it again during cleanup.
            }
            catch (Exception cleanupFailure) when (!assertionsPassed)
            {
                Console.Error.WriteLine($"Dispatch cleanup also failed: {cleanupFailure}");
            }
        }
    }

    [Test]
    [Property("TestCaseId", "CORE-003-AC-02")]
    public async Task Dispatch_UnregisteredConsumer_FaultsWithoutHandlingPayload()
    {
        using var provider = CreateServices()
            .AddConsumer<Message, RecordingConsumer>("consumer")
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<ConsumerDispatcher>();
        var consumer = scope.ServiceProvider.GetRequiredService<RecordingConsumer>();
        var data = provider.GetRequiredService<IMessageSerializer>().Serialize(new Message("payload"));

        var exception = await Assert.That(async () =>
            await dispatcher.Dispatch("missing", data, default))
            .Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("consumer.not_registered");
        await Assert.That(exception.Action).IsEqualTo(FailureAction.Fault);
        await Assert.That(consumer.Messages).IsEmpty();
    }

    [Test]
    public async Task Dispatch_SeparateScopes_UsesIsolatedScopedConsumers()
    {
        using var provider = CreateServices()
            .AddConsumer<Message, RecordingConsumer>("consumer")
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();
        var data = provider.GetRequiredService<IMessageSerializer>().Serialize(new Message("payload"));

        await first.ServiceProvider.GetRequiredService<ConsumerDispatcher>().Dispatch("consumer", data, default);

        await Assert.That(first.ServiceProvider.GetRequiredService<RecordingConsumer>().Messages.ToArray())
            .IsEquivalentTo(new[] { new Message("payload") });
        await Assert.That(second.ServiceProvider.GetRequiredService<RecordingConsumer>().Messages).IsEmpty();
    }

    [Test]
    public async Task Dispatch_AmbientContext_ExposesIdentityAcrossAwaitWithoutReplacingScope()
    {
        using var provider = CreateServices()
            .AddConsumer<Message, ContextConsumer>("consumer")
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = provider.GetRequiredService<MessageContext>();
        var messageId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        var data = provider.GetRequiredService<IMessageSerializer>().Serialize(new Message("payload"));

        using (context.Push(messageId, correlationId, causationId, "consumer"))
        {
            await scope.ServiceProvider.GetRequiredService<ConsumerDispatcher>()
                .Dispatch("consumer", data, default);

            var consumer = scope.ServiceProvider.GetRequiredService<ContextConsumer>();
            await Assert.That(consumer.MessageId).IsEqualTo((Guid?)messageId);
            await Assert.That(consumer.CorrelationId).IsEqualTo((Guid?)correlationId);
            await Assert.That(consumer.CausationId).IsEqualTo((Guid?)causationId);
            await Assert.That(consumer.ConsumerId).IsEqualTo("consumer");
            await Assert.That(context.MessageId).IsEqualTo((Guid?)messageId);
        }
        await Assert.That(context.MessageId).IsNull();
    }

    private static IServiceCollection CreateServices()
        => new ServiceCollection().AddContract<Message>("message.v1");

    private sealed record Message(string Value);

    private sealed class AwaitingConsumer : IMessageConsumer<Message>
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Message? Message { get; private set; }
        public CancellationToken Token { get; private set; }

        public async ValueTask Handle(Message message, CancellationToken cancellationToken)
        {
            Message = message;
            Token = cancellationToken;
            Started.SetResult();
            await Release.Task.WaitAsync(cancellationToken);
        }
    }

    private sealed class FailingConsumer(Exception cause) : IMessageConsumer<Message>
    {
        public async ValueTask Handle(Message message, CancellationToken cancellationToken)
        {
            await Task.Yield();
            throw cause;
        }
    }

    private sealed class RecordingConsumer : IMessageConsumer<Message>
    {
        public RecordingConsumer() { }

        public List<Message> Messages { get; } = [];
        public ValueTask Handle(Message message, CancellationToken cancellationToken)
        {
            Messages.Add(message);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ContextConsumer(IMessageContext context) : IMessageConsumer<Message>
    {
        public Guid? MessageId { get; private set; }
        public Guid? CorrelationId { get; private set; }
        public Guid? CausationId { get; private set; }
        public string? ConsumerId { get; private set; }
        public async ValueTask Handle(Message message, CancellationToken cancellationToken)
        {
            await Task.Yield();
            MessageId = context.MessageId;
            CorrelationId = context.CorrelationId;
            CausationId = context.CausationId;
            ConsumerId = context.ConsumerId;
        }
    }
}

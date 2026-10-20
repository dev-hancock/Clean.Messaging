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
        bool completedBeforeRelease;

        try
        {
            await consumer.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
            completedBeforeRelease = dispatch.IsCompleted;
        }
        finally
        {
            consumer.Release.TrySetResult();
            await Task.WhenAll(dispatch, consumer.Completed.Task).WaitAsync(TimeSpan.FromSeconds(10));
        }

        await Assert.That(completedBeforeRelease).IsFalse();
        await Assert.That(consumer.Message).IsEqualTo(message);
        await Assert.That(consumer.Token).IsEqualTo(cancellation.Token);
    }

    [Test]
    [Property("TestCaseId", "CORE-003-AC-04")]
    public async Task Dispatch_ConsumerFailure_ExposesHandlerFailure()
    {
        var cause = new InvalidOperationException("controlled consumer failure");
        var consumer = new FailingConsumer(cause);
        using var provider = CreateServices()
            .AddConsumer<Message, FailingConsumer>("consumer")
            .AddSingleton(consumer)
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<ConsumerDispatcher>();
        var message = new Message("payload");
        var data = provider.GetRequiredService<IMessageSerializer>().Serialize(message);

        await Assert.That(async () => await dispatcher.Dispatch("consumer", data, default))
            .Throws<InvalidOperationException>();

        await Assert.That(consumer.Message).IsEqualTo(message);
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
        bool completedBeforeCancellation;
        OperationCanceledException? exception;

        try
        {
            await consumer.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
            completedBeforeCancellation = dispatch.IsCompleted;
            cancellation.Cancel();
            exception = await Assert.That(() => dispatch.WaitAsync(TimeSpan.FromSeconds(10)))
                .Throws<OperationCanceledException>();
        }
        finally
        {
            consumer.Release.TrySetResult();
            await consumer.Completed.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }

        await Assert.That(completedBeforeCancellation).IsFalse();
        await Assert.That(exception).IsNotNull();
        await Assert.That(exception.CancellationToken).IsEqualTo(cancellation.Token);
        await Assert.That(consumer.Token).IsEqualTo(cancellation.Token);
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
        using var second = provider.CreateScope();
        var serializer = provider.GetRequiredService<IMessageSerializer>();
        var firstMessage = new Message("first scope");
        var secondMessage = new Message("second scope");
        RecordingConsumer firstConsumer;
        var secondConsumer = second.ServiceProvider.GetRequiredService<RecordingConsumer>();

        using (var first = provider.CreateScope())
        {
            firstConsumer = first.ServiceProvider.GetRequiredService<RecordingConsumer>();
            await first.ServiceProvider.GetRequiredService<ConsumerDispatcher>()
                .Dispatch("consumer", serializer.Serialize(firstMessage), default);
            await second.ServiceProvider.GetRequiredService<ConsumerDispatcher>()
                .Dispatch("consumer", serializer.Serialize(secondMessage), default);
        }
        var laterMessage = new Message("second scope after first disposed");
        await second.ServiceProvider.GetRequiredService<ConsumerDispatcher>()
            .Dispatch("consumer", serializer.Serialize(laterMessage), default);

        await Assert.That(firstConsumer.Messages.ToArray()).IsEquivalentTo(new[] { firstMessage });
        await Assert.That(firstConsumer.IsDisposed).IsTrue();
        await Assert.That(secondConsumer.Messages.ToArray()).IsEquivalentTo(new[] { secondMessage, laterMessage });
        await Assert.That(secondConsumer.IsDisposed).IsFalse();
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
            await Assert.That(context.CorrelationId).IsEqualTo((Guid?)correlationId);
            await Assert.That(context.CausationId).IsEqualTo((Guid?)causationId);
            await Assert.That(context.ConsumerId).IsEqualTo("consumer");
        }
        await Assert.That(context.MessageId).IsNull();
        await Assert.That(context.CorrelationId).IsNull();
        await Assert.That(context.CausationId).IsNull();
        await Assert.That(context.ConsumerId).IsNull();
    }

    private static IServiceCollection CreateServices()
        => new ServiceCollection().AddContract<Message>("message.v1");

    private sealed record Message(string Value);

    private sealed class AwaitingConsumer : IMessageConsumer<Message>
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Completed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Message? Message { get; private set; }
        public CancellationToken Token { get; private set; }

        public async ValueTask Handle(Message message, CancellationToken cancellationToken)
        {
            Message = message;
            Token = cancellationToken;
            Started.SetResult();
            try
            {
                await Release.Task.WaitAsync(cancellationToken);
            }
            finally
            {
                Completed.TrySetResult();
            }
        }
    }

    private sealed class FailingConsumer(Exception cause) : IMessageConsumer<Message>
    {
        public Message? Message { get; private set; }

        public async ValueTask Handle(Message message, CancellationToken cancellationToken)
        {
            await Task.Yield();
            Message = message;
            throw cause;
        }
    }

    private sealed class RecordingConsumer : IMessageConsumer<Message>, IDisposable
    {
        public RecordingConsumer() { }

        public List<Message> Messages { get; } = [];
        public bool IsDisposed { get; private set; }

        public ValueTask Handle(Message message, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            Messages.Add(message);
            return ValueTask.CompletedTask;
        }

        public void Dispose() => IsDisposed = true;
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

using Clean.Messaging.Abstractions;
using Clean.Messaging.Consumers;
using Clean.Messaging.Exceptions;
using Clean.Messaging.Processing;
using Clean.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Tests.Configuration;

public sealed class ConsumerValidationTests
{
    [Test]
    [Property("TestCaseId", "CONFIG-004-AC-01")]
    public async Task StartAsync_MissingConsumerContract_FailsValidation()
    {
        var consumer = new Consumer();
        using var provider = new ServiceCollection().AddSingleton(consumer)
            .AddConsumer<Message, Consumer>("consumer").BuildServiceProvider();

        var exception = await Assert.That(() => StartConsumers(provider)).Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("contract.type.not_registered");
        await Assert.That(exception.Action).IsEqualTo(FailureAction.Fault);
        await Assert.That(consumer.Messages).IsEmpty();
    }

    [Test]
    [Property("TestCaseId", "CONFIG-004-AC-02")]
    public async Task StartAsync_MissingConsumerService_FailsValidation()
    {
        var invoker = new UnregisteredConsumerInvoker();
        var services = new ServiceCollection().AddContract<Message>("message.v1");
        services.AddSingleton<IConsumerInvoker>(invoker);
        using var provider = services.BuildServiceProvider();

        await Assert.That(() => StartConsumers(provider)).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task StartAsync_CompleteConsumerRegistration_AllowsDispatchAfterValidation()
    {
        using var provider = new ServiceCollection()
            .AddContract<Message>("message.v1")
            .AddConsumer<Message, Consumer>("consumer")
            .BuildServiceProvider();

        var validation = provider.GetServices<IHostedService>().Single();
        var message = new Message("validated consumer");
        var data = provider.GetRequiredService<IMessageSerializer>().Serialize(message);

        await validation.StartAsync(default);
        try
        {
            using var scope = provider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ConsumerDispatcher>().Dispatch("consumer", data, default);

            await Assert.That(scope.ServiceProvider.GetRequiredService<Consumer>().Messages.ToArray())
                .IsEquivalentTo(new[] { message });
        }
        finally
        {
            await validation.StopAsync(default);
        }
    }

    private static async Task StartConsumers(ServiceProvider provider)
    {
        foreach (var service in provider.GetServices<IHostedService>())
        {
            await service.StartAsync(default);
        }
    }

    private sealed record Message(string Value);

    private sealed class Consumer : IMessageConsumer<Message>
    {
        public Consumer() { }

        public List<Message> Messages { get; } = [];

        public ValueTask Handle(Message message, CancellationToken cancellationToken)
        {
            Messages.Add(message);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class UnregisteredConsumerInvoker : IConsumerInvoker
    {
        public string ConsumerId => "missing-service";
        public Type MessageType => typeof(Message);
        public Type ConsumerType => typeof(Consumer);

        public ValueTask Invoke(IServiceProvider services, object message, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Startup validation must reject the missing consumer service.");
    }
}

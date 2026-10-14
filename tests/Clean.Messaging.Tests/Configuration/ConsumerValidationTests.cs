using Clean.Messaging.Abstractions;
using Clean.Messaging.Consumers;
using Clean.Messaging.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Tests.Configuration;

public sealed class ConsumerValidationTests
{
    [Test]
    [Property("TestCaseId", "CONFIG-004-AC-01")]
    public async Task StartAsync_MissingConsumerContract_FailsValidation()
    {
        using var provider = new ServiceCollection().AddConsumer<Message, Consumer>("consumer").BuildServiceProvider();

        var exception = await Assert.That(() => StartConsumers(provider)).Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("contract.type.not_registered");
    }

    [Test]
    [Property("TestCaseId", "CONFIG-004-AC-02")]
    public async Task StartAsync_MissingConsumerService_FailsValidation()
    {
        var invoker = new UnregisteredConsumerInvoker();
        var services = new ServiceCollection().AddContract<Message>("message.v1");
        services.AddSingleton<IConsumerInvoker>(invoker);
        using var provider = services.BuildServiceProvider();

        var exception = await Assert.That(() => StartConsumers(provider)).Throws<InvalidOperationException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Message).Contains("missing-service");
    }

    [Test]
    public async Task StartAsync_CompleteConsumerRegistration_CompletesValidation()
    {
        using var provider = new ServiceCollection()
            .AddContract<Message>("message.v1")
            .AddConsumer<Message, Consumer>("consumer")
            .BuildServiceProvider();

        foreach (var service in provider.GetServices<IHostedService>())
        {
            await service.StartAsync(default);
            await service.StopAsync(default);
        }

        await Assert.That(provider.GetRequiredService<IConsumerRegistry>().Get("consumer").MessageType)
            .IsEqualTo(typeof(Message));
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

        public ValueTask Handle(Message message, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;
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

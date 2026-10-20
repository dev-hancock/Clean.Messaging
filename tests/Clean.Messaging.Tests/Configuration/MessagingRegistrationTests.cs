using Clean.Messaging.Abstractions;
using Clean.Messaging.Consumers;
using Clean.Messaging.Contracts;
using Clean.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Clean.Messaging.Tests.Configuration;

public sealed class MessagingRegistrationTests
{
    [Test]
    [Property("TestCaseId", "CONFIG-002-AC-01")]
    public async Task AddMessaging_PreRegisteredOverrides_PreservesHostImplementations()
    {
        var clock = Substitute.For<TimeProvider>();
        var context = Substitute.For<IMessageContext>();
        var consumers = Substitute.For<IConsumerRegistry>();
        var contracts = Substitute.For<IMessageContractRegistry>();
        var serializer = Substitute.For<IMessageSerializer>();
        var services = new ServiceCollection();
        services.AddSingleton(clock);
        services.AddSingleton(context);
        services.AddSingleton(consumers);
        services.AddSingleton(contracts);
        services.AddSingleton(serializer);
        services.AddMessaging();
        using var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetRequiredService<TimeProvider>()).IsSameReferenceAs(clock);
        await Assert.That(provider.GetRequiredService<IMessageContext>()).IsSameReferenceAs(context);
        await Assert.That(provider.GetRequiredService<IConsumerRegistry>()).IsSameReferenceAs(consumers);
        await Assert.That(provider.GetRequiredService<IMessageContractRegistry>()).IsSameReferenceAs(contracts);
        await Assert.That(provider.GetRequiredService<IMessageSerializer>()).IsSameReferenceAs(serializer);
    }

    [Test]
    [Property("TestCaseId", "CONFIG-002-AC-02")]
    public async Task AddMessaging_RepeatedRegistration_ResolvesSingleDefaultCapabilities()
    {
        using var provider = new ServiceCollection().AddMessaging()
            .AddContract<Message>("message.v1").AddConsumer<Message, Consumer>("consumer")
            .AddMessaging().BuildServiceProvider();
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();

        await Assert.That(provider.GetServices<TimeProvider>().Count()).IsEqualTo(1);
        await Assert.That(provider.GetServices<IMessageContext>().Count()).IsEqualTo(1);
        await Assert.That(provider.GetServices<IConsumerRegistry>().Count()).IsEqualTo(1);
        await Assert.That(provider.GetServices<IMessageContractRegistry>().Count()).IsEqualTo(1);
        await Assert.That(provider.GetServices<IMessageSerializer>().Count()).IsEqualTo(1);
        await Assert.That(first.ServiceProvider.GetRequiredService<TimeProvider>())
            .IsSameReferenceAs(second.ServiceProvider.GetRequiredService<TimeProvider>());
        await Assert.That(first.ServiceProvider.GetRequiredService<IMessageContext>())
            .IsSameReferenceAs(second.ServiceProvider.GetRequiredService<IMessageContext>());
        await Assert.That(first.ServiceProvider.GetRequiredService<IConsumerRegistry>())
            .IsSameReferenceAs(second.ServiceProvider.GetRequiredService<IConsumerRegistry>());
        await Assert.That(first.ServiceProvider.GetRequiredService<IMessageContractRegistry>())
            .IsSameReferenceAs(second.ServiceProvider.GetRequiredService<IMessageContractRegistry>());
        await Assert.That(first.ServiceProvider.GetRequiredService<IMessageSerializer>())
            .IsSameReferenceAs(second.ServiceProvider.GetRequiredService<IMessageSerializer>());
        await Assert.That(provider.GetRequiredService<IConsumerRegistry>().All.Select(x => x.ConsumerId).ToArray())
            .IsEquivalentTo(new[] { "consumer" });
        var contracts = provider.GetRequiredService<IMessageContractRegistry>();
        await Assert.That(contracts.GetContract(typeof(Message))).IsEqualTo("message.v1");
        await Assert.That(contracts.GetType("message.v1")).IsEqualTo(typeof(Message));
    }

    private sealed record Message(string Value);

    private sealed class Consumer : IMessageConsumer<Message>
    {
        public Consumer() { }

        public ValueTask Handle(Message message, CancellationToken cancellationToken) => ValueTask.CompletedTask;
    }
}

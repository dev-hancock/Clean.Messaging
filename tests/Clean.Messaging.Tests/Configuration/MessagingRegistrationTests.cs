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

        await Assert.That(ReferenceEquals(provider.GetRequiredService<TimeProvider>(), clock)).IsTrue();
        await Assert.That(ReferenceEquals(provider.GetRequiredService<IMessageContext>(), context)).IsTrue();
        await Assert.That(ReferenceEquals(provider.GetRequiredService<IConsumerRegistry>(), consumers)).IsTrue();
        await Assert.That(ReferenceEquals(provider.GetRequiredService<IMessageContractRegistry>(), contracts)).IsTrue();
        await Assert.That(ReferenceEquals(provider.GetRequiredService<IMessageSerializer>(), serializer)).IsTrue();
    }

    [Test]
    [Property("TestCaseId", "CONFIG-002-AC-02")]
    public async Task AddMessaging_RepeatedRegistration_ResolvesSingleDefaultCapabilities()
    {
        using var provider = new ServiceCollection().AddMessaging().AddMessaging().BuildServiceProvider();

        await Assert.That(provider.GetServices<IMessageContext>().Count()).IsEqualTo(1);
        await Assert.That(provider.GetServices<IConsumerRegistry>().Count()).IsEqualTo(1);
        await Assert.That(provider.GetServices<IMessageContractRegistry>().Count()).IsEqualTo(1);
        await Assert.That(provider.GetServices<IMessageSerializer>().Count()).IsEqualTo(1);
        await Assert.That(provider.GetRequiredService<IConsumerRegistry>().All).IsEmpty();
        await Assert.That(ReferenceEquals(provider.GetRequiredService<IMessageContext>(), provider.GetRequiredService<IMessageContext>()))
            .IsTrue();
    }
}

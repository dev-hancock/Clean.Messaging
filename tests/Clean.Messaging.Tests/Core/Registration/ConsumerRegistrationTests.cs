using Clean.Messaging.Abstractions;
using Clean.Messaging.Consumers;
using Clean.Messaging.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Tests.Core.Registration;

public sealed class ConsumerRegistrationTests
{
    [Test]
    [Property("TestCaseId", "CONFIG-003-AC-02")]
    public async Task AddConsumer_MaximumLengthIdentifier_AcceptsRegistration()
    {
        var id = new string('a', 200);
        using var provider = new ServiceCollection().AddConsumer<Message, Consumer>(id).BuildServiceProvider();

        await Assert.That(provider.GetRequiredService<IConsumerRegistry>().Get(id).ConsumerId).IsEqualTo(id);
    }

    [Test]
    [Property("TestCaseId", "CONFIG-003-AC-02")]
    public async Task AddConsumer_OverlengthIdentifier_RejectsRegistration()
    {
        var id = new string('a', 201);

        await Assert.That(() => new ServiceCollection().AddConsumer<Message, Consumer>(id))
            .Throws<ArgumentException>();
    }

    [Test]
    [Property("TestCaseId", "CONFIG-003-AC-02")]
    public async Task AddConsumer_NonAsciiIdentifier_RejectsRegistration()
    {
        const string id = "consumer.\u00e9";

        await Assert.That(() => new ServiceCollection().AddConsumer<Message, Consumer>(id))
            .Throws<ArgumentException>();
    }

    [Test]
    [Property("TestCaseId", "CONFIG-003-AC-03")]
    [Arguments(null)]
    [Arguments("")]
    [Arguments(" \t")]
    public async Task AddConsumer_BlankIdentifier_RejectsRegistration(string? id)
    {
        await Assert.That(() => new ServiceCollection().AddConsumer<Message, Consumer>(id!))
            .Throws<ArgumentException>();
    }

    [Test]
    [Property("TestCaseId", "CORE-002-AC-02")]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Get_RepeatedConsumerIdentity_RejectsRegistryAcrossMessageTypes(bool differentType)
    {
        var services = new ServiceCollection().AddConsumer<Message, Consumer>("consumer");
        if (differentType)
        {
            services.AddConsumer<DerivedMessage, DerivedConsumer>("consumer");
        }
        else
        {
            services.AddConsumer<Message, Consumer>("consumer");
        }
        using var provider = services.BuildServiceProvider();

        await Assert.That(() => provider.GetRequiredService<IConsumerRegistry>())
            .Throws<InvalidOperationException>();
    }

    [Test]
    [Property("TestCaseId", "CORE-002-AC-03")]
    public async Task Get_ExactMessageType_ReturnsOnlyMatchingConsumers()
    {
        using var provider = new ServiceCollection()
            .AddConsumer<Message, Consumer>("base")
            .AddConsumer<DerivedMessage, DerivedConsumer>("derived")
            .BuildServiceProvider();
        var registry = provider.GetRequiredService<IConsumerRegistry>();

        await Assert.That(registry.Get(typeof(Message)).Select(x => x.ConsumerId).ToArray())
            .IsEquivalentTo(new[] { "base" });
        await Assert.That(registry.Get(typeof(DerivedMessage)).Select(x => x.ConsumerId).ToArray())
            .IsEquivalentTo(new[] { "derived" });
    }

    [Test]
    [Property("TestCaseId", "CORE-002-AC-03")]
    public async Task Get_BaseConsumerOnly_FaultsForUnregisteredDerivedType()
    {
        using var provider = new ServiceCollection()
            .AddConsumer<Message, Consumer>("base")
            .BuildServiceProvider();
        var registry = provider.GetRequiredService<IConsumerRegistry>();

        var exception = await Assert.That(() => registry.Get(typeof(DerivedMessage)))
            .Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("consumer.message_unhandled");
        await Assert.That(exception.Action).IsEqualTo(FailureAction.Fault);
    }

    [Test]
    [Property("TestCaseId", "CORE-002-AC-04")]
    public async Task Get_CaseDistinctIdentities_UsesOrdinalLookup()
    {
        using var provider = new ServiceCollection()
            .AddConsumer<Message, Consumer>("consumer")
            .AddConsumer<Message, Consumer>("Consumer")
            .BuildServiceProvider();
        var registry = provider.GetRequiredService<IConsumerRegistry>();

        await Assert.That(registry.Get("consumer").ConsumerId).IsEqualTo("consumer");
        await Assert.That(registry.Get("Consumer").ConsumerId).IsEqualTo("Consumer");
        var exception = await Assert.That(() => registry.Get("CONSUMER")).Throws<MessageException>();
        await Assert.That(exception).IsNotNull();
        await Assert.That(exception.Code).IsEqualTo("consumer.not_registered");
    }

    [Test]
    [Property("TestCaseId", "CORE-002-AC-05")]
    public async Task Get_MultipleConsumers_ReturnsCompleteSetWithoutOrderingRequirement()
    {
        using var provider = new ServiceCollection()
            .AddConsumer<Message, Consumer>("consumer")
            .AddConsumer<Message, Consumer>("Consumer")
            .BuildServiceProvider();
        var registry = provider.GetRequiredService<IConsumerRegistry>();

        await Assert.That(registry.Get(typeof(Message)).Select(x => x.ConsumerId).ToArray())
            .IsEquivalentTo(new[] { "consumer", "Consumer" });
        await Assert.That(registry.All.Select(x => x.ConsumerId).ToArray())
            .IsEquivalentTo(new[] { "consumer", "Consumer" });
    }

    [Test]
    [Property("TestCaseId", "CORE-003-AC-02")]
    public async Task Get_UnregisteredConsumerId_ReportsPermanentResolutionFailure()
    {
        using var provider = new ServiceCollection().AddMessaging().BuildServiceProvider();
        var registry = provider.GetRequiredService<IConsumerRegistry>();

        var byId = await Assert.That(() => registry.Get("missing")).Throws<MessageException>();
        await Assert.That(byId).IsNotNull();

        await Assert.That(byId.Code).IsEqualTo("consumer.not_registered");
        await Assert.That(byId.Action).IsEqualTo(FailureAction.Fault);
    }

    [Test]
    public async Task Get_UnregisteredMessageType_ReportsPermanentResolutionFailure()
    {
        using var provider = new ServiceCollection().AddMessaging().BuildServiceProvider();
        var registry = provider.GetRequiredService<IConsumerRegistry>();

        var exception = await Assert.That(() => registry.Get(typeof(Message))).Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("consumer.message_unhandled");
        await Assert.That(exception.Action).IsEqualTo(FailureAction.Fault);
    }

    [Test]
    [Property("TestCaseId", "CORE-003-AC-03")]
    public async Task Invoke_WrongMessageType_FaultsWithoutHandlingMessage()
    {
        var consumer = new Consumer();
        using var provider = new ServiceCollection()
            .AddSingleton(consumer)
            .AddConsumer<Message, Consumer>("consumer")
            .BuildServiceProvider();
        var invoker = provider.GetRequiredService<IConsumerRegistry>().Get("consumer");

        var exception = await Assert.That(async () => await invoker.Invoke(provider, new OtherMessage("incompatible"), default))
            .Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("consumer.message_type_mismatch");
        await Assert.That(exception.Action).IsEqualTo(FailureAction.Fault);
        await Assert.That(consumer.Messages).IsEmpty();
    }

    [Test]
    [Property("TestCaseId", "CONFIG-002-AC-01")]
    public async Task AddConsumer_PreRegisteredConsumer_PreservesHostService()
    {
        var consumer = new Consumer();
        var services = new ServiceCollection();
        services.AddSingleton(consumer);
        services.AddConsumer<Message, Consumer>("consumer");
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        await Assert.That(ReferenceEquals(scope.ServiceProvider.GetRequiredService<Consumer>(), consumer)).IsTrue();
        await Assert.That(provider.GetServices<Consumer>().Count()).IsEqualTo(1);
    }

    private record Message(string Value);
    private sealed record DerivedMessage(string Value) : Message(Value);
    private sealed record OtherMessage(string Value);

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

    private sealed class DerivedConsumer : IMessageConsumer<DerivedMessage>
    {
        public DerivedConsumer() { }

        public ValueTask Handle(DerivedMessage message, CancellationToken cancellationToken) => ValueTask.CompletedTask;
    }
}

using Clean.Messaging.Contracts;
using Clean.Messaging.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Tests.Core.Registration;

public sealed class ContractRegistrationTests
{
    [Test]
    [Property("TestCaseId", "CORE-001-AC-01")]
    public async Task GetContract_RegisteredMessage_ResolvesBothDirections()
    {
        var services = new ServiceCollection().AddContract<Message>("message.v1");
        using var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<IMessageContractRegistry>();

        await Assert.That(registry.GetContract(typeof(Message))).IsEqualTo("message.v1");
        await Assert.That(registry.GetType("message.v1")).IsEqualTo(typeof(Message));
    }

    [Test]
    [Property("TestCaseId", "CONFIG-003-AC-01")]
    public async Task AddContract_MaximumLengthIdentifier_AcceptsRegistration()
    {
        var name = new string('a', 500);
        using var provider = new ServiceCollection().AddContract<Message>(name).BuildServiceProvider();

        await Assert.That(provider.GetRequiredService<IMessageContractRegistry>().GetType(name))
            .IsEqualTo(typeof(Message));
    }

    [Test]
    [Property("TestCaseId", "CONFIG-003-AC-01")]
    public async Task AddContract_OverlengthIdentifier_RejectsRegistration()
    {
        await Assert.That(() => new ServiceCollection().AddContract<Message>(new string('a', 501)))
            .Throws<ArgumentException>();
    }

    [Test]
    [Property("TestCaseId", "CONFIG-003-AC-03")]
    [Arguments(null)]
    [Arguments("")]
    [Arguments(" \t")]
    public async Task AddContract_BlankIdentifier_RejectsRegistration(string? name)
    {
        await Assert.That(() => new ServiceCollection().AddContract<Message>(name!))
            .Throws<ArgumentException>();
    }

    [Test]
    [Property("TestCaseId", "CORE-002-AC-01")]
    [Arguments("message.v1")]
    [Arguments("another.v1")]
    public async Task GetContract_RepeatedMessageType_RejectsAmbiguousRegistry(string secondName)
    {
        var services = new ServiceCollection()
            .AddContract<Message>("message.v1")
            .AddContract<Message>(secondName);
        using var provider = services.BuildServiceProvider();

        await Assert.That(() => provider.GetRequiredService<IMessageContractRegistry>())
            .Throws<InvalidOperationException>();
    }

    [Test]
    [Property("TestCaseId", "CORE-002-AC-01")]
    public async Task GetType_RepeatedContractName_RejectsAmbiguousRegistry()
    {
        var services = new ServiceCollection()
            .AddContract<Message>("message.v1")
            .AddContract<OtherMessage>("message.v1");
        using var provider = services.BuildServiceProvider();

        await Assert.That(() => provider.GetRequiredService<IMessageContractRegistry>())
            .Throws<InvalidOperationException>();
    }

    [Test]
    [Property("TestCaseId", "CORE-002-AC-04")]
    public async Task GetType_CaseDistinctNames_ResolvesDistinctContracts()
    {
        using var provider = new ServiceCollection()
            .AddContract<Message>("message.v1")
            .AddContract<OtherMessage>("Message.v1")
            .BuildServiceProvider();
        var registry = provider.GetRequiredService<IMessageContractRegistry>();

        await Assert.That(registry.GetType("message.v1")).IsEqualTo(typeof(Message));
        await Assert.That(registry.GetType("Message.v1")).IsEqualTo(typeof(OtherMessage));
    }

    [Test]
    [Property("TestCaseId", "CORE-003-AC-01")]
    public async Task GetContract_UnregisteredType_ReportsPermanentResolutionFailure()
    {
        using var provider = new ServiceCollection().AddMessaging().BuildServiceProvider();
        var registry = provider.GetRequiredService<IMessageContractRegistry>();

        var exception = await Assert.That(() => registry.GetContract(typeof(Message)))
            .Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("contract.type.not_registered");
        await Assert.That(exception.Action).IsEqualTo(FailureAction.Fault);
    }

    [Test]
    [Property("TestCaseId", "CORE-003-AC-01")]
    public async Task GetType_UnregisteredName_ReportsPermanentResolutionFailure()
    {
        using var provider = new ServiceCollection().AddContract<Message>("message.v1").BuildServiceProvider();
        var registry = provider.GetRequiredService<IMessageContractRegistry>();

        var exception = await Assert.That(() => registry.GetType("MESSAGE.V1"))
            .Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("contract.identifier.not_registered");
        await Assert.That(exception.Action).IsEqualTo(FailureAction.Fault);
    }

    private sealed record Message(string Value);
    private sealed record OtherMessage(string Value);
}

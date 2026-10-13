using Clean.Messaging.Exceptions;
using Clean.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Clean.Messaging.Tests.Core.Serialization;

public sealed class MessageSerializationTests
{
    [Test]
    [Property("TestCaseId", "CORE-001-AC-02")]
    public async Task Deserialize_RetainedContractPayload_RestoresCompatibleMessage()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var serializer = provider.GetRequiredService<IMessageSerializer>();

        var payload = await File.ReadAllTextAsync(Path.Combine(
            AppContext.BaseDirectory, "Core", "Serialization", "Fixtures", "message.v1.json"));

        var message = serializer.Deserialize(new MessageData("message.v1", payload));

        await Assert.That(message).IsEqualTo(new Message("retained", 3));
    }

    [Test]
    [Property("TestCaseId", "CORE-004-AC-02")]
    public async Task Serialize_ConcreteMessage_UsesRegisteredRuntimeContractAndWebDefaults()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var serializer = provider.GetRequiredService<IMessageSerializer>();
        object message = new Message("sample", 4);

        var data = serializer.Serialize(message);

        await Assert.That(data.Type).IsEqualTo("message.v1");
        using var json = JsonDocument.Parse(data.Value);
        await Assert.That(json.RootElement.GetProperty("displayName").GetString()).IsEqualTo("sample");
        await Assert.That(json.RootElement.GetProperty("count").GetInt32()).IsEqualTo(4);
        await Assert.That(serializer.Deserialize(data)).IsEqualTo(message);
    }

    [Test]
    [Property("TestCaseId", "CORE-004-AC-01")]
    public async Task Serialize_DerivedMessage_UsesConcreteContractAndPayload()
    {
        using var provider = new ServiceCollection()
            .AddContract<BaseMessage>("base.v1")
            .AddContract<DerivedMessage>("derived.v1")
            .BuildServiceProvider();
        var serializer = provider.GetRequiredService<IMessageSerializer>();
        BaseMessage message = new DerivedMessage("value", 7);

        var data = serializer.Serialize(message);

        await Assert.That(data.Type).IsEqualTo("derived.v1");
        await Assert.That(serializer.Deserialize(data)).IsEqualTo(message);
    }

    [Test]
    [Property("TestCaseId", "CORE-004-AC-03")]
    public async Task Serialize_SuppliedJsonOptions_UsesCustomNamingAndConverter()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        options.Converters.Add(new JsonStringEnumConverter());
        var services = new ServiceCollection().AddContract<ConfiguredMessage>("configured.v1");
        services.AddSingleton(options);
        using var provider = services.BuildServiceProvider();
        var serializer = provider.GetRequiredService<IMessageSerializer>();
        var message = new ConfiguredMessage("sample", MessageState.Ready);

        var data = serializer.Serialize(message);

        using var json = JsonDocument.Parse(data.Value);
        await Assert.That(json.RootElement.GetProperty("display_name").GetString()).IsEqualTo("sample");
        await Assert.That(json.RootElement.GetProperty("state").GetString()).IsEqualTo("Ready");
        await Assert.That(serializer.Deserialize(data)).IsEqualTo(message);
    }

    [Test]
    [Property("TestCaseId", "CORE-004-AC-04")]
    [Arguments("{")]
    [Arguments("{\"displayName\":\"sample\",\"count\":\"not-a-number\"}")]
    public async Task Deserialize_InvalidJson_ReportsPermanentFailureWithCause(string json)
    {
        using var provider = CreateServices().BuildServiceProvider();
        var serializer = provider.GetRequiredService<IMessageSerializer>();

        var exception = await Assert.That(() => serializer.Deserialize(new MessageData("message.v1", json)))
            .Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("serialization.deserialize.failed");
        await Assert.That(exception.Action).IsEqualTo(FailureAction.Fault);
        await Assert.That(exception.InnerException).IsTypeOf<JsonException>();
    }

    [Test]
    [Property("TestCaseId", "CORE-004-AC-04")]
    public async Task Deserialize_JsonNull_ReportsPermanentFailure()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var serializer = provider.GetRequiredService<IMessageSerializer>();

        var exception = await Assert.That(() => serializer.Deserialize(new MessageData("message.v1", "null")))
            .Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("serialization.deserialize.failed");
        await Assert.That(exception.Action).IsEqualTo(FailureAction.Fault);
    }

    [Test]
    [Property("TestCaseId", "CORE-004-AC-04")]
    public async Task Deserialize_UnsupportedMessage_ReportsPermanentFailureWithCause()
    {
        using var provider = new ServiceCollection().AddContract<Action>("action.v1").BuildServiceProvider();
        var serializer = provider.GetRequiredService<IMessageSerializer>();

        var exception = await Assert.That(() => serializer.Deserialize(new MessageData("action.v1", "{}")))
            .Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("serialization.deserialize.failed");
        await Assert.That(exception.Action).IsEqualTo(FailureAction.Fault);
        await Assert.That(exception.InnerException).IsTypeOf<NotSupportedException>();
    }

    [Test]
    [Property("TestCaseId", "CORE-004-AC-05")]
    public async Task Serialize_UnsupportedMessage_ReportsPermanentFailureWithCause()
    {
        using var provider = new ServiceCollection().AddContract<Action>("action.v1").BuildServiceProvider();
        var serializer = provider.GetRequiredService<IMessageSerializer>();
        Action message = () => { };

        var exception = await Assert.That(() => serializer.Serialize(message)).Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("serialization.serialize.failed");
        await Assert.That(exception.Action).IsEqualTo(FailureAction.Fault);
        await Assert.That(exception.InnerException).IsTypeOf<NotSupportedException>();
    }

    [Test]
    [Property("TestCaseId", "CORE-004-AC-05")]
    public async Task Serialize_ConverterJsonFailure_PreservesOriginalCause()
    {
        var cause = new JsonException("controlled conversion failure");
        var options = new JsonSerializerOptions();
        options.Converters.Add(new FailingConverter(cause));
        var services = CreateServices();
        services.AddSingleton(options);
        using var provider = services.BuildServiceProvider();
        var serializer = provider.GetRequiredService<IMessageSerializer>();

        var exception = await Assert.That(() => serializer.Serialize(new Message("sample", 1)))
            .Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("serialization.serialize.failed");
        await Assert.That(exception.Action).IsEqualTo(FailureAction.Fault);
        await Assert.That(ReferenceEquals(exception.InnerException, cause)).IsTrue();
    }

    [Test]
    [Property("TestCaseId", "CORE-004-AC-06")]
    public async Task Serialize_NullMessage_RejectsInput()
    {
        using var provider = new ServiceCollection().AddMessaging().BuildServiceProvider();
        var serializer = provider.GetRequiredService<IMessageSerializer>();

        await Assert.That(() => serializer.Serialize(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    [Property("TestCaseId", "CORE-003-AC-01")]
    public async Task Deserialize_UnknownContract_PreservesResolutionFailure()
    {
        using var provider = new ServiceCollection().AddMessaging().BuildServiceProvider();
        var serializer = provider.GetRequiredService<IMessageSerializer>();

        var exception = await Assert.That(() => serializer.Deserialize(new MessageData("missing.v1", "{}")))
            .Throws<MessageException>();
        await Assert.That(exception).IsNotNull();

        await Assert.That(exception.Code).IsEqualTo("contract.identifier.not_registered");
        await Assert.That(exception.Action).IsEqualTo(FailureAction.Fault);
    }

    private static IServiceCollection CreateServices()
        => new ServiceCollection().AddContract<Message>("message.v1");

    private sealed record Message(string DisplayName, int Count);
    private record BaseMessage(string Value);
    private sealed record DerivedMessage(string Value, int Count) : BaseMessage(Value);
    private enum MessageState { Ready }
    private sealed record ConfiguredMessage(string DisplayName, MessageState State);

    private sealed class FailingConverter(JsonException cause) : JsonConverter<Message>
    {
        public override Message Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw cause;
        public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
            => throw cause;
    }
}

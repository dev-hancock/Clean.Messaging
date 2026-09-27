using System.Text.Json;

namespace Clean.Messaging.Transport;

public interface ITransportSerializer
{
    ReadOnlyMemory<byte> Serialize<TMessage>(
        TMessage message)
        where TMessage : notnull;

    TMessage Deserialize<TMessage>(
        ReadOnlyMemory<byte> payload)
        where TMessage : notnull;
}

internal sealed class JsonTransportSerializer(
    JsonSerializerOptions options)
    : ITransportSerializer
{
    public ReadOnlyMemory<byte> Serialize<TMessage>(
        TMessage message)
        where TMessage : notnull
    {
        return JsonSerializer.SerializeToUtf8Bytes(
            message,
            options);
    }

    public TMessage Deserialize<TMessage>(
        ReadOnlyMemory<byte> payload)
        where TMessage : notnull
    {
        var message = JsonSerializer.Deserialize<TMessage>(
            payload.Span,
            options);

        return message ?? throw new InvalidOperationException(
            $"Transport payload deserialized to null for '{typeof(TMessage)}'.");
    }
}

public static class TransportSerializers
{
    public static ITransportSerializer Json { get; } =
        new JsonTransportSerializer(
            new JsonSerializerOptions(
                JsonSerializerDefaults.Web));
}
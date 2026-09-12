using System.Text.Json;
using Clean.Messaging.Exceptions;

namespace Clean.Messaging.Serialization;

internal interface IMessageSerializer
{
    MessageData Serialize(object message);

    object Deserialize(MessageData data);
}

internal sealed class MessageSerializer(
    IMessageContractRegistry contracts,
    JsonSerializerOptions? options = null) : IMessageSerializer
{
    private readonly JsonSerializerOptions _options =
        options ?? new(JsonSerializerDefaults.Web);

    public MessageData Serialize(object message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var type = message.GetType();
        var contract = contracts.GetContract(type);

        try
        {
            var value = JsonSerializer.Serialize(
                message,
                type,
                _options);

            return new(contract, value);
        }
        catch (Exception exception)
            when (exception is JsonException or NotSupportedException)
        {
            throw new MessageException(
                ErrorCodes.Serialization.SerializeFailed,
                $"Failed to serialize message contract '{contract}'.",
                FailureAction.Fault,
                exception);
        }
    }

    public object Deserialize(MessageData data)
    {
        var type = contracts.GetType(data.Type);

        try
        {
            var message = JsonSerializer.Deserialize(
                data.Value,
                type,
                _options);

            return message ?? throw new MessageException(
                ErrorCodes.Serialization.DeserializeFailed,
                $"Failed to deserialize message contract '{data.Type}'.",
                FailureAction.Fault);
        }
        catch (MessageException)
        {
            throw;
        }
        catch (Exception exception)
            when (exception is JsonException or NotSupportedException)
        {
            throw new MessageException(
                ErrorCodes.Serialization.DeserializeFailed,
                $"Failed to deserialize message contract '{data.Type}'.",
                FailureAction.Fault,
                exception);
        }
    }
}
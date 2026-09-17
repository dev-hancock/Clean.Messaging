using Clean.Messaging.Sagas.Configuration;
using Clean.Messaging.Sagas.Exceptions;
using Clean.Messaging.Sagas.Persistence;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Clean.Messaging.Sagas.Runtime;

internal sealed class SagaStateSerializer(
    IOptions<SagaOptions> options)
{
    private readonly JsonSerializerOptions _options =
        options.Value.SerializerOptions;

    public string Serialize<TState>(
        TState state)
        where TState : class
    {
        try
        {
            return JsonSerializer.Serialize(
                state,
                _options);
        }
        catch (Exception exception)
            when (exception is JsonException or NotSupportedException)
        {
            throw new SagaSerializationException(
                $"Saga state '{typeof(TState)}' could not be serialized.",
                exception);
        }
    }

    public TState Deserialize<TState>(
        SagaEntry saga)
        where TState : class
    {
        try
        {
            return JsonSerializer.Deserialize<TState>(
                       saga.State,
                       _options)
                   ?? throw new SagaSerializationException(
                       $"Saga '{saga.Type}' state deserialized to null.");
        }
        catch (SagaSerializationException)
        {
            throw;
        }
        catch (Exception exception)
            when (exception is JsonException or NotSupportedException)
        {
            throw new SagaSerializationException(
                $"Saga '{saga.Type}' state could not be deserialized.",
                exception);
        }
    }
}
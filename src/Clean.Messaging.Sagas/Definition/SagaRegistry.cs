using Clean.Messaging.Sagas.Exceptions;
using Clean.Messaging.Serialization;
using System.Collections.Frozen;

namespace Clean.Messaging.Sagas.Definition;

internal sealed class SagaRegistry
{
    private readonly FrozenDictionary<Type, ISagaDefinition> _byState;
    private readonly FrozenDictionary<string, ISagaDefinition> _byType;

    public SagaRegistry(
        IEnumerable<ISagaDefinition> definitions,
        IMessageContractRegistry contracts)
    {
        var byState = new Dictionary<Type, ISagaDefinition>();
        var byType = new Dictionary<string, ISagaDefinition>(StringComparer.Ordinal);

        foreach (var definition in definitions)
        {
            if (!byState.TryAdd(
                    definition.StateType,
                    definition))
            {
                throw new InvalidOperationException(
                    $"Saga state '{definition.StateType}' is registered more than once.");
            }

            if (!byType.TryAdd(
                    definition.Type,
                    definition))
            {
                throw new InvalidOperationException(
                    $"Saga type '{definition.Type}' is registered more than once.");
            }
        }

        ValidateContracts(
            byState.Values,
            contracts);

        _byState = byState.ToFrozenDictionary();
        _byType = byType.ToFrozenDictionary(StringComparer.Ordinal);
    }

    private static void ValidateContracts(
        IEnumerable<ISagaDefinition> definitions,
        IMessageContractRegistry contracts)
    {
        var messageTypes = definitions
            .SelectMany(definition =>
                definition.MessageTypes)
            .Distinct();

        foreach (var messageType in messageTypes)
        {
            _ = contracts.GetContract(messageType);
        }
    }

    public SagaDefinition<TState> Get<TState>()
        where TState : class
    {
        if (_byState.TryGetValue(typeof(TState), out var definition) &&
            definition is SagaDefinition<TState> typed)
        {
            return typed;
        }

        throw new InvalidOperationException(
            $"Saga state '{typeof(TState)}' is not registered.");
    }

    public ISagaDefinition Get(
        string sagaType)
    {
        if (_byType.TryGetValue(sagaType, out var definition))
        {
            return definition;
        }

        throw new SagaSerializationException(
            $"Saga type '{sagaType}' is not registered.");
    }
}
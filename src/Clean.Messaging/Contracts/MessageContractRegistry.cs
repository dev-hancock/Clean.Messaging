using Clean.Messaging.Exceptions;
using System.Collections.Frozen;

namespace Clean.Messaging.Contracts;

public sealed record MessageContract
{
    public required string Name { get; init; }

    public required Type Type { get; init; }
}

public interface IMessageContractRegistry
{
    string GetContract(Type type);

    Type GetType(string contract);
}

internal sealed class MessageContractRegistry : IMessageContractRegistry
{
    private readonly FrozenDictionary<Type, string> _byType;
    private readonly FrozenDictionary<string, Type> _byName;

    public MessageContractRegistry(
        IEnumerable<MessageContract> contracts)
    {
        ArgumentNullException.ThrowIfNull(contracts);

        (_byType, _byName) = Build(contracts);
    }

    public string GetContract(Type type)
    {
        if (_byType.TryGetValue(type, out var contract))
        {
            return contract;
        }

        throw new MessageException(
            ErrorCodes.Contract.TypeNotRegistered,
            $"Message type '{type}' has no registered contract.",
            FailureAction.Fault);
    }

    public Type GetType(string contract)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contract);

        if (_byName.TryGetValue(contract, out var type))
        {
            return type;
        }

        throw new MessageException(
            ErrorCodes.Contract.IdentifierNotRegistered,
            $"Message contract '{contract}' is not registered.",
            FailureAction.Fault);
    }

    private static (
        FrozenDictionary<Type, string> ByType,
        FrozenDictionary<string, Type> ByName)
        Build(IEnumerable<MessageContract> contracts)
    {
        var byType = new Dictionary<Type, string>();
        var byName = new Dictionary<string, Type>(StringComparer.Ordinal);

        foreach (var contract in contracts)
        {
            if (!byType.TryAdd(contract.Type, contract.Name))
            {
                throw new InvalidOperationException(
                    $"Message type '{contract.Type}' is registered more than once.");
            }

            if (!byName.TryAdd(contract.Name, contract.Type))
            {
                throw new InvalidOperationException(
                    $"Message contract '{contract.Name}' is registered more than once.");
            }
        }

        return (
            byType.ToFrozenDictionary(),
            byName.ToFrozenDictionary(StringComparer.Ordinal));
    }
}
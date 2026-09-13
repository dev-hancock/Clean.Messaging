using Clean.Messaging.Exceptions;
using System.Collections.Frozen;

namespace Clean.Messaging.Serialization;

internal sealed record MessageContractRegistration(
    Type Type,
    string Contract);

public interface IMessageContractRegistry
{
    string GetContract(Type type);

    Type GetType(string contract);
}

internal sealed class MessageContractRegistry : IMessageContractRegistry
{
    private readonly FrozenDictionary<Type, string> _contractsByType;
    private readonly FrozenDictionary<string, Type> _typesByContract;

    public MessageContractRegistry(
        IEnumerable<MessageContractRegistration> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);

        (_contractsByType, _typesByContract) = Build(registrations);
    }

    public string GetContract(Type type)
    {
        if (_contractsByType.TryGetValue(type, out var contract))
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

        if (_typesByContract.TryGetValue(contract, out var type))
        {
            return type;
        }

        throw new MessageException(
            ErrorCodes.Contract.IdentifierNotRegistered,
            $"Message contract '{contract}' is not registered.",
            FailureAction.Fault);
    }

    private static (
        FrozenDictionary<Type, string> ContractsByType,
        FrozenDictionary<string, Type> TypesByContract)
        Build(IEnumerable<MessageContractRegistration> registrations)
    {
        var contractsByType = new Dictionary<Type, string>();
        var typesByContract = new Dictionary<string, Type>(StringComparer.Ordinal);

        foreach (var registration in registrations)
        {
            if (!contractsByType.TryAdd(registration.Type, registration.Contract))
            {
                throw new InvalidOperationException(
                    $"Message type '{registration.Type}' is registered more than once.");
            }

            if (!typesByContract.TryAdd(registration.Contract, registration.Type))
            {
                throw new InvalidOperationException(
                    $"Message contract '{registration.Contract}' is registered more than once.");
            }
        }

        return (
            contractsByType.ToFrozenDictionary(),
            typesByContract.ToFrozenDictionary(StringComparer.Ordinal));
    }
}
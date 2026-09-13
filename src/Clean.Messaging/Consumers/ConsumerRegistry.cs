using Clean.Messaging.Abstractions;
using Clean.Messaging.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;

namespace Clean.Messaging.Consumers;

public interface IConsumerInvoker
{
    string ConsumerId { get; }

    Type MessageType { get; }

    Type ConsumerType { get; }

    ValueTask Invoke(
        IServiceProvider services,
        object message,
        CancellationToken cancellationToken);
}

internal sealed class ConsumerInvoker<TMessage, TConsumer>(string consumerId)
    : IConsumerInvoker
    where TMessage : notnull
    where TConsumer : class, IMessageConsumer<TMessage>
{
    public string ConsumerId { get; } = consumerId;

    public Type MessageType => typeof(TMessage);

    public Type ConsumerType => typeof(TConsumer);

    public ValueTask Invoke(
        IServiceProvider services,
        object message,
        CancellationToken cancellationToken)
    {
        if (message is not TMessage typed)
        {
            throw new MessageException(
                "consumer.message_type_mismatch",
                $"Consumer '{ConsumerId}' expects '{MessageType}' but received '{message.GetType()}'.",
                FailureAction.Fault);
        }

        var consumer = services.GetRequiredService<TConsumer>();

        return consumer.Handle(typed, cancellationToken);
    }
}

public interface IConsumerRegistry
{
    IEnumerable<IConsumerInvoker> All { get; }

    IReadOnlyList<IConsumerInvoker> Get(Type messageType);

    IConsumerInvoker Get(string consumerId);
}

internal sealed class ConsumerRegistry : IConsumerRegistry
{
    private readonly FrozenDictionary<string, IConsumerInvoker> _consumers;
    private readonly FrozenDictionary<Type, IConsumerInvoker[]> _consumersByMessage;

    public ConsumerRegistry(IEnumerable<IConsumerInvoker> registrations)
    {
        var consumers = new Dictionary<string, IConsumerInvoker>(StringComparer.Ordinal);

        foreach (var registration in registrations)
        {
            if (!consumers.TryAdd(registration.ConsumerId, registration))
            {
                throw new InvalidOperationException(
                    $"Consumer identity '{registration.ConsumerId}' is registered more than once.");
            }
        }

        _consumers = consumers.ToFrozenDictionary(StringComparer.Ordinal);
        _consumersByMessage = consumers.Values
            .GroupBy(consumer => consumer.MessageType)
            .ToFrozenDictionary(
                group => group.Key,
                group => group.ToArray());
    }

    public IEnumerable<IConsumerInvoker> All => _consumers.Values;

    public IReadOnlyList<IConsumerInvoker> Get(Type messageType)
    {
        if (_consumersByMessage.TryGetValue(messageType, out var consumers))
        {
            return consumers;
        }

        throw new MessageException(
            "consumer.message_unhandled",
            $"Message type '{messageType}' has no registered consumers.",
            FailureAction.Fault);
    }

    public IConsumerInvoker Get(string consumerId)
    {
        if (_consumers.TryGetValue(consumerId, out var consumer))
        {
            return consumer;
        }

        throw new MessageException(
            "consumer.not_registered",
            $"Consumer '{consumerId}' is not registered.",
            FailureAction.Fault);
    }
}

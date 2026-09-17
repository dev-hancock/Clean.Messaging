using Clean.Messaging.Sagas.Definition;
using Clean.Messaging.Sagas.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Clean.Messaging.Sagas;

public sealed class SagaBuilder
{
    private readonly IServiceCollection _services;

    private readonly HashSet<Type> _states = [];

    private readonly HashSet<string> _types = new(StringComparer.Ordinal);

    internal SagaBuilder(
        IServiceCollection services)
    {
        _services = services;
    }

    internal int Count => _states.Count;

    public SagaBuilder Add<TState>(
        string type,
        Action<SagaBuilder<TState>> configure)
        where TState : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentNullException.ThrowIfNull(configure);

        if (type.Length > 200 ||
            type.Any(character => character > 127))
        {
            throw new ArgumentException(
                "Saga type identifiers must be at most 200 ASCII characters.",
                nameof(type));
        }

        if (!_states.Add(typeof(TState)))
        {
            throw new InvalidOperationException(
                $"Saga state '{typeof(TState)}' is registered more than once.");
        }

        if (!_types.Add(type))
        {
            throw new InvalidOperationException(
                $"Saga type '{type}' is registered more than once.");
        }

        var builder = new SagaBuilder<TState>(
            _services,
            type);

        configure(builder);

        _services.AddSingleton<ISagaDefinition>(
            builder.Build());

        return this;
    }
}

public sealed class SagaBuilder<TState>
    where TState : class
{
    private readonly Dictionary<Type, ISagaMessageDefinition<TState>> _messages = [];

    private readonly IServiceCollection _services;

    private readonly string _type;

    internal SagaBuilder(
        IServiceCollection services,
        string type)
    {
        _services = services;
        _type = type;
    }

    public SagaBuilder<TState> On<TMessage, THandler>(
        string consumerId,
        Func<TMessage, SagaKey> correlate,
        Func<TMessage, TState> create)
        where TMessage : notnull
        where THandler : class, ISagaHandler<TState, TMessage>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            consumerId);

        ArgumentNullException.ThrowIfNull(
            correlate);

        ArgumentNullException.ThrowIfNull(
            create);

        AddMessage(
            new SagaMessageDefinition<
                TState,
                TMessage,
                THandler>(
                correlate,
                create));

        AddConsumer<TMessage, THandler>(
            consumerId);

        return this;
    }

    public SagaBuilder<TState> On<TMessage, THandler>(
        string consumerId,
        Func<TMessage, SagaKey> correlate)
        where TMessage : notnull
        where THandler : class, ISagaHandler<TState, TMessage>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            consumerId);

        ArgumentNullException.ThrowIfNull(
            correlate);

        AddMessage(
            new SagaMessageDefinition<
                TState,
                TMessage,
                THandler>(
                correlate));

        AddConsumer<TMessage, THandler>(
            consumerId);

        return this;
    }

    public SagaBuilder<TState> On<TMessage, THandler>()
        where TMessage : notnull
        where THandler : class, ISagaHandler<TState, TMessage>
    {
        AddMessage(
            new SagaMessageDefinition<
                TState,
                TMessage,
                THandler>());

        _services.TryAddScoped<THandler>();

        return this;
    }

    internal SagaDefinition<TState> Build()
    {
        if (_messages.Count == 0)
        {
            throw new InvalidOperationException(
                $"Saga '{_type}' has no messages.");
        }

        return new(
            _type,
            new Dictionary<
                Type,
                ISagaMessageDefinition<TState>>(
                _messages));
    }

    private void AddMessage(
        ISagaMessageDefinition<TState> message)
    {
        if (_messages.TryAdd(
                message.MessageType,
                message))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Saga '{_type}' already handles '{message.MessageType}'.");
    }

    private void AddConsumer<TMessage, THandler>(
        string consumerId)
        where TMessage : notnull
        where THandler : class, ISagaHandler<TState, TMessage>
    {
        _services.TryAddScoped<THandler>();

        _services.AddConsumer<
            TMessage,
            SagaConsumer<TState, TMessage>>(
            consumerId);
    }
}
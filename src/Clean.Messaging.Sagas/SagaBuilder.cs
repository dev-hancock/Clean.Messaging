using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Clean.Messaging.Sagas;

public sealed class SagaBuilder<TState>
    where TState : class
{
    private readonly Dictionary<Type, ISagaMessageRegistration<TState>> _messages = [];
    private readonly IServiceCollection _services;
    private readonly Dictionary<Type, ISagaTimerRegistration<TState>> _timersByType = [];
    private readonly string _type;

    internal SagaBuilder(
        IServiceCollection services,
        string type)
    {
        _services = services;
        _type = type;
    }

    public SagaBuilder<TState> StartsWith<TMessage, THandler>(
        string consumerId,
        Func<TMessage, SagaKey> correlate,
        Func<TMessage, TState> create)
        where TMessage : notnull
        where THandler : class, ISagaHandler<TState, TMessage>
    {
        ArgumentNullException.ThrowIfNull(correlate);
        ArgumentNullException.ThrowIfNull(create);

        AddMessage(
            new SagaStartRegistration<TState, TMessage, THandler>(
                correlate,
                create));

        AddConsumer<TMessage, THandler>(consumerId);

        return this;
    }

    public SagaBuilder<TState> Handles<TMessage, THandler>(
        string consumerId,
        Func<TMessage, SagaKey> correlate)
        where TMessage : notnull
        where THandler : class, ISagaHandler<TState, TMessage>
    {
        ArgumentNullException.ThrowIfNull(correlate);

        AddMessage(
            new SagaHandleRegistration<TState, TMessage, THandler>(
                correlate));

        AddConsumer<TMessage, THandler>(consumerId);

        return this;
    }

    public SagaBuilder<TState> HandlesTimer<TMessage, THandler>()
        where TMessage : notnull
        where THandler : class, ISagaHandler<TState, TMessage>
    {
        var registration =
            new SagaTimerRegistration<TState, TMessage, THandler>();

        if (!_timersByType.TryAdd(
                typeof(TMessage),
                registration))
        {
            throw new InvalidOperationException(
                $"Saga '{_type}' already has a timer handler for '{typeof(TMessage)}'.");
        }

        _services.TryAddScoped<THandler>();

        return this;
    }

    internal SagaDefinition<TState> Build()
    {
        if (_messages.Count == 0 &&
            _timersByType.Count == 0)
        {
            throw new InvalidOperationException(
                $"Saga '{_type}' has no message or timer handlers.");
        }

        return new(
            _type,
            new Dictionary<Type, ISagaMessageRegistration<TState>>(_messages),
            new Dictionary<Type, ISagaTimerRegistration<TState>>(_timersByType));
    }

    private void AddMessage<TMessage>(
        ISagaMessageRegistration<TState, TMessage> registration)
        where TMessage : notnull
    {
        if (!_messages.TryAdd(
                typeof(TMessage),
                registration))
        {
            throw new InvalidOperationException(
                $"Saga '{_type}' already has a handler for '{typeof(TMessage)}'.");
        }
    }

    private void AddConsumer<TMessage, THandler>(
        string consumerId)
        where TMessage : notnull
        where THandler : class, ISagaHandler<TState, TMessage>
    {
        _services.TryAddScoped<THandler>();

        _services.AddConsumer<
            TMessage,
            SagaConsumer<TState, TMessage>>(consumerId);
    }
}
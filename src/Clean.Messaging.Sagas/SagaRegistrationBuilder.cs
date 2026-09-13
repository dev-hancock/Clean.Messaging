using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Sagas;

public sealed class SagaRegistrationBuilder
{
    private readonly IServiceCollection _services;
    private readonly HashSet<Type> _states = [];
    private readonly HashSet<string> _types = new(StringComparer.Ordinal);

    internal SagaRegistrationBuilder(
        IServiceCollection services)
    {
        _services = services;
    }

    internal int Count => _states.Count;

    public SagaRegistrationBuilder Add<TState>(
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
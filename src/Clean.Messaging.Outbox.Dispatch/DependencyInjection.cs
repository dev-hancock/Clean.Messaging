using Clean.Messaging.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Clean.Messaging.Outbox.Dispatch;

public static class DependencyInjection
{
    public static IServiceCollection AddDispatch(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(
            services);

        services.TryAddSingleton(
            TransportSerializers.Json);

        services.TryAddScoped<
            TransportMessageFactory>();

        services.TryAddScoped<
            TransportDispatchRegistry>();

        services.TryAddScoped<
            IOutboxDispatch,
            OutboxDispatch>();

        return services;
    }

    public static IServiceCollection AddDispatch<TMessage>(
        this IServiceCollection services)
        where TMessage : notnull
    {
        return services.AddDispatch<
            TMessage,
            TMessage,
            TransportMapper<TMessage>>();
    }

    public static IServiceCollection AddDispatch<
        TMessage,
        TPayload,
        TMapper>(
        this IServiceCollection services)
        where TMessage : notnull
        where TPayload : notnull
        where TMapper :
            class,
            ITransportMapper<TMessage, TPayload>
    {
        ArgumentNullException.ThrowIfNull(
            services);

        services.AddDispatch();

        services.TryAddScoped<TMapper>();

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<
                ITransportDispatch,
                TransportDispatch<
                    TMessage,
                    TPayload,
                    TMapper>>());

        return services;
    }
}
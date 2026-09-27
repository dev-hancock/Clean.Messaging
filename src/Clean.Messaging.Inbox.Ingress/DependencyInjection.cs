using Clean.Messaging.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Clean.Messaging.Inbox.Ingress;

public static class DependencyInjection
{
    public static IServiceCollection AddIngress(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(
            services);

        services.TryAddSingleton<ITransportSerializer>(
            TransportSerializers.Json);

        services.TryAddSingleton<
            InboxMessageFactory>();

        services.TryAddScoped<
            TransportIngressRegistry>();

        services.TryAddScoped<
            IInboxIngress,
            InboxIngress>();

        return services;
    }

    public static IServiceCollection AddIngress<TMessage>(
        this IServiceCollection services)
        where TMessage : notnull
    {
        return services.AddIngress<
            TMessage,
            TMessage,
            TransportMapper<TMessage>>();
    }

    public static IServiceCollection AddIngress<
        TPayload,
        TMessage,
        TMapper>(
        this IServiceCollection services)
        where TPayload : notnull
        where TMessage : notnull
        where TMapper :
        class,
        ITransportMapper<TMessage>
    {
        services.AddIngress();

        services.TryAddScoped<TMapper>();

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<
                ITransportIngress,
                TransportIngress<
                    TPayload,
                    TMessage,
                    TMapper>>());

        return services;
    }
}
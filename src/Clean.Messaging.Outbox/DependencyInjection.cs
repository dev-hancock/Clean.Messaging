using Clean.Messaging.Outbox.Delivery;
using Clean.Messaging.Outbox.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Clean.Messaging.Outbox;

public static class DependencyInjection
{
    public static IServiceCollection AddOutboxTransport<TTransport>(
        this IServiceCollection services)
        where TTransport : class, IOutboxTransport
    {
        ArgumentNullException.ThrowIfNull(
            services);

        services.TryAddSingleton<TTransport>();

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<
                IOutboxDelivery,
                OutboxTransport<TTransport>>());

        return services;
    }
}
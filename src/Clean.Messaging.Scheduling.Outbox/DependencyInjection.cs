using Clean.Messaging.Scheduling.Delivery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Scheduling.Outbox;

public static class DependencyInjection
{
    public static IServiceCollection AddOutboxScheduling(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScheduling();

        services.TryAddScoped<
            IMessageScheduler,
            OutboxMessageScheduler>();

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<
                IScheduleDelivery,
                OutboxScheduleDelivery>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                OutboxSchedulingValidation>());

        return services;
    }
}

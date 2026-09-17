using Clean.Messaging.Scheduling.Configuration;
using Clean.Messaging.Scheduling.Delivery;
using Clean.Messaging.Scheduling.Persistence;
using Clean.Messaging.Scheduling.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Scheduling;

public static class DependencyInjection
{
    public static IServiceCollection AddScheduling(
        this IServiceCollection services,
        Action<SchedulingOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMessaging();

        var options = services.AddOptions<SchedulingOptions>();

        if (configure is not null)
        {
            options.Configure(configure);
        }

        options.ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IValidateOptions<SchedulingOptions>,
                SchedulingOptionsValidator>());

        services.TryAddSingleton<
            ISchedulingEngine,
            SchedulingEngine>();

        services.TryAddScoped<
            IScheduleWriter,
            ScheduleWriter>();

        services.TryAddScoped<
            ScheduleDeliveryRegistry>();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                SchedulingRuntimeValidation>());

        return services;
    }
}

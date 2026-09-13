using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            IScheduledMessageWriter,
            ScheduledMessageWriter>();

        services.TryAddScoped<
            ScheduledMessageDeliveryRegistry>();

        return services;
    }
}

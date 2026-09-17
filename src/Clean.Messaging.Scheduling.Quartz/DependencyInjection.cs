using Clean.Messaging.Scheduling.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;

namespace Clean.Messaging.Scheduling.Quartz;

public static class DependencyInjection
{
    public static IServiceCollection UseQuartzScheduling(
        this IServiceCollection services,
        Action<IQuartzBuilder>? configure = null)
    {
        services.AddScheduling();

        var hasDefaultScheduler = services.Any(descriptor =>
            descriptor.ServiceType == typeof(IScheduler) &&
            !descriptor.IsKeyedService);

        if (hasDefaultScheduler)
        {
            if (configure is not null)
            {
                services.ConfigureAllQuartzSchedulers(builder =>
                {
                    if (string.IsNullOrEmpty(builder.SchedulerName))
                    {
                        configure(builder);
                    }
                });
            }
        }
        else
        {
            services.AddQuartz(configure);
        }

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        services.RemoveAll<ISchedulingEngine>();
        services.TryAddSingleton<SchedulingEngine>();
        services.TryAddSingleton<ISchedulingEngine>(provider =>
            provider.GetRequiredService<SchedulingEngine>());

        return services;
    }
}

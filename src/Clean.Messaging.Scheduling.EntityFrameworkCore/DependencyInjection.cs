using Clean.Messaging.EntityFrameworkCore;
using Clean.Messaging.Scheduling.Configuration;
using Clean.Messaging.Scheduling.Persistence;
using Clean.Messaging.Scheduling.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Scheduling.EntityFrameworkCore;

public static class DependencyInjection
{
    public static MessagingBuilder<TDbContext> AddScheduling<TDbContext>(
        this MessagingBuilder<TDbContext> builder,
        Action<SchedulingOptions>? configure = null)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddScheduling(configure);

        builder.Services.TryAddScoped<
            IScheduleStore,
            ScheduleStore<TDbContext>>();

        builder.Services.TryAddScoped<
            IScheduleReader,
            ScheduleStore<TDbContext>>();

        builder.Services.TryAddScoped<
            IScheduleExecutor,
            ScheduleExecutor<TDbContext>>();

        builder.Services.TryAddScoped<SchedulingTracker>();
        builder.Services.TryAddScoped<SchedulingSaveInterceptor>();
        builder.Services.TryAddScoped<SchedulingTransactionInterceptor>();

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                Runtime.SchedulingWorker>());

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                ScheduleCleanupWorker>());

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                SchedulingValidation<TDbContext>>());

        return builder;
    }

    public static ModelBuilder AddScheduling(
        this ModelBuilder modelBuilder)
    {
        SchedulingConfiguration.Configure(modelBuilder);

        return modelBuilder;
    }

    public static DbContextOptionsBuilder AddScheduling(
        this DbContextOptionsBuilder options,
        IServiceProvider services)
    {
        var save = services.GetRequiredService<
            SchedulingSaveInterceptor>();

        var transaction = services.GetRequiredService<
            SchedulingTransactionInterceptor>();

        AddInterceptor(
            options,
            save);

        AddInterceptor(
            options,
            transaction);

        return options;
    }

    public static DbContextOptionsBuilder<TDbContext> AddScheduling<TDbContext>(
        this DbContextOptionsBuilder<TDbContext> options,
        IServiceProvider services)
        where TDbContext : DbContext
    {
        ((DbContextOptionsBuilder)options)
            .AddScheduling(services);

        return options;
    }

    private static void AddInterceptor(
        DbContextOptionsBuilder options,
        IInterceptor interceptor)
    {
        var existing = options.Options
            .FindExtension<CoreOptionsExtension>()
            ?.Interceptors;

        if (existing?.Any(candidate =>
                ReferenceEquals(
                    candidate,
                    interceptor)) == true)
        {
            return;
        }

        options.AddInterceptors(interceptor);
    }
}

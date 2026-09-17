using Clean.Messaging.EntityFrameworkCore;
using Clean.Messaging.Sagas.Configuration;
using Clean.Messaging.Sagas.Persistence;
using Clean.Messaging.Scheduling.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Sagas.EntityFrameworkCore;

public static class DependencyInjection
{
    public static MessagingBuilder<TDbContext> AddSagas<TDbContext>(
        this MessagingBuilder<TDbContext> builder,
        Action<SagaBuilder> configure,
        Action<SagaOptions>? configureOptions = null)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        builder.AddScheduling();

        builder.Services.AddSagas(
            configure,
            configureOptions);

        builder.Services.TryAddScoped<
            ISagaStore,
            SagaStore<TDbContext>>();

        builder.Services.TryAddScoped<
            ISagaConflictResolver,
            SagaConflictResolver<TDbContext>>();

        builder.Services.TryAddScoped<SagaSaveInterceptor>();

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                SagaCleanupWorker>());

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                SagaValidation<TDbContext>>());

        return builder;
    }

    public static ModelBuilder AddSagas(
        this ModelBuilder modelBuilder)
    {
        modelBuilder.AddScheduling();

        SagaConfiguration.Configure(modelBuilder);

        return modelBuilder;
    }

    public static DbContextOptionsBuilder AddSagas(
        this DbContextOptionsBuilder options,
        IServiceProvider services)
    {
        options.AddScheduling(services);

        var interceptor = services.GetRequiredService<
            SagaSaveInterceptor>();

        AddInterceptor(
            options,
            interceptor);

        return options;
    }

    public static DbContextOptionsBuilder<TDbContext> AddSagas<TDbContext>(
        this DbContextOptionsBuilder<TDbContext> options,
        IServiceProvider services)
        where TDbContext : DbContext
    {
        ((DbContextOptionsBuilder)options)
            .AddSagas(services);

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

using Clean.Messaging.DeadLetters;
using Clean.Messaging.EntityFrameworkCore;
using Clean.Messaging.Persistence;
using Clean.Messaging.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Outbox.EntityFrameworkCore;

public static class DependencyInjection
{
    public static IServiceCollection AddOutbox<TDbContext>(
        this IServiceCollection services,
        Action<OutboxOptions>? configure = null)
        where TDbContext : DbContext
    {
        services
            .AddMessaging();

        PersistenceServices
            .EnsurePersistence<TDbContext>(
                services);

        PersistenceServices.AddOptions(
            services,
            configure);

        services.TryAddSingleton<
            OutboxSignal>();

        services.TryAddSingleton<
            WorkLoop<OutboxEntry>>();

        services.TryAddSingleton<
            OutboxEntryFactory>();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IOutboxTargetProvider,
                OutboxTargetProvider>());

        services.TryAddSingleton<
            IOutboxWriter,
            OutboxWriter>();

        services.TryAddScoped<
            IOutbox,
            Outbox>();

        services.TryAddScoped<
            IOutboxStore,
            OutboxStore<TDbContext>>();

        services.TryAddScoped<
            IDurableStore<OutboxEntry>>(provider =>
                provider.GetRequiredService<IOutboxStore>());

        services.TryAddScoped<
            IDeadLetterStore<OutboxEntry>,
            DeadLetterStore<TDbContext, OutboxEntry>>();

        services.TryAddScoped<
            IOutboxManager,
            OutboxManager>();

        services.TryAddScoped<
            OutboxProcessor>();

        services.TryAddScoped<
            IWorkProcessor<OutboxEntry>>(provider =>
                provider.GetRequiredService<OutboxProcessor>());

        services.TryAddScoped<
            IOutboxDispatcher,
            OutboxDispatcher>();

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<
                IOutboxDelivery,
                LocalOutboxDelivery<TDbContext>>());

        services.TryAddScoped<
            OutboxTracker>();

        services.TryAddScoped<
            IOutboxCommitVerifier,
            OutboxCommitVerifier<TDbContext>>();

        services.TryAddScoped<
            OutboxInterceptor>();

        services.TryAddScoped<
            OutboxTransactionInterceptor>();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                OutboxWorker>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                OutboxRecoveryWorker>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                OutboxCleanupWorker>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                OutboxValidation<TDbContext>>());

        return services;
    }

    public static IServiceCollection AddOutbox<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(
            configuration);

        return services.AddOutbox<TDbContext>(
            options =>
                configuration
                    .GetSection(OutboxOptions.Section)
                    .Bind(options));
    }

    public static MessagingBuilder<TDbContext> AddOutbox<TDbContext>(
        this MessagingBuilder<TDbContext> builder,
        Action<OutboxOptions>? configure = null)
        where TDbContext : DbContext
    {
        builder.Services.AddOutbox<TDbContext>(
            configure);

        return builder;
    }

    public static DbContextOptionsBuilder AddMessaging(
        this DbContextOptionsBuilder options,
        IServiceProvider services)
    {
        var interceptor =
            services.GetService<OutboxInterceptor>();

        if (interceptor is null)
        {
            return options;
        }

        return options.AddInterceptors(
            interceptor,
            services.GetRequiredService<OutboxTransactionInterceptor>());
    }

    public static DbContextOptionsBuilder<TDbContext> AddMessaging<TDbContext>(
        this DbContextOptionsBuilder<TDbContext> options,
        IServiceProvider services)
        where TDbContext : DbContext
    {
        ((DbContextOptionsBuilder)options)
            .AddMessaging(services);

        return options;
    }

    public static ModelBuilder AddOutbox(
        this ModelBuilder modelBuilder)
    {
        OutboxConfiguration.Configure(
            modelBuilder);

        return modelBuilder;
    }
}
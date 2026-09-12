using Clean.Messaging.DeadLetters;
using Clean.Messaging.EntityFrameworkCore;
using Clean.Messaging.Persistence;
using Clean.Messaging.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Inbox.EntityFrameworkCore;

public static class DependencyInjection
{
    public static IServiceCollection AddInbox<TDbContext>(
        this IServiceCollection services,
        Action<InboxOptions>? configure = null)
        where TDbContext : DbContext
    {
        services.AddMessaging();

        PersistenceServices.EnsurePersistence<TDbContext>(services);
        PersistenceServices.AddOptions(services, configure);

        services.TryAddSingleton<InboxSignal>();
        services.TryAddSingleton<WorkLoop<InboxEntry>>();

        services.TryAddScoped<IInbox, Inbox>();
        services.TryAddScoped<IInboxStore, InboxStore<TDbContext>>();
        services.TryAddScoped<IDurableStore<InboxEntry>>(provider =>
            provider.GetRequiredService<IInboxStore>());

        services.TryAddScoped<
            IDeadLetterStore<InboxEntry>,
            DeadLetterStore<TDbContext, InboxEntry>>();

        services.TryAddScoped<IInboxManager, InboxManager>();

        services.TryAddScoped<InboxProcessor>();
        services.TryAddScoped<IWorkProcessor<InboxEntry>>(provider =>
            provider.GetRequiredService<InboxProcessor>());

        services.TryAddScoped<IInboxDispatcher, InboxDispatcher<TDbContext>>();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, InboxWorker>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, InboxRecovery>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, InboxCleanup>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, InboxValidation<TDbContext>>());

        return services;
    }

    public static IServiceCollection AddInbox<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddInbox<TDbContext>(options =>
            configuration
                .GetSection(InboxOptions.Section)
                .Bind(options));
    }

    public static MessagingBuilder<TDbContext> AddInbox<TDbContext>(
        this MessagingBuilder<TDbContext> builder,
        Action<InboxOptions>? configure = null)
        where TDbContext : DbContext
    {
        builder.Services.AddInbox<TDbContext>(configure);

        return builder;
    }

    public static ModelBuilder AddInbox(
        this ModelBuilder modelBuilder)
    {
        InboxConfiguration.Configure(modelBuilder);

        return modelBuilder;
    }
}

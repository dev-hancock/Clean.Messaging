using Clean.Messaging.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.EntityFrameworkCore;

internal sealed record MessagingPersistence(Type DbContextType);

internal static class PersistenceServices
{
    public static void EnsurePersistence<TDbContext>(
        IServiceCollection services)
        where TDbContext : DbContext
    {
        var registration = services
            .FirstOrDefault(service =>
                service.ServiceType == typeof(MessagingPersistence))
            ?.ImplementationInstance as MessagingPersistence;

        if (registration is not null &&
            registration.DbContextType != typeof(TDbContext))
        {
            throw new InvalidOperationException(
                $"Messaging is already configured for '{registration.DbContextType}'. " +
                "Inbox and outbox must use the same scoped context type.");
        }

        services.TryAddSingleton(
            new MessagingPersistence(typeof(TDbContext)));
    }

    public static void AddOptions<TOptions>(
        IServiceCollection services,
        Action<TOptions>? configure)
        where TOptions : MessageOptions, new()
    {
        var options = services.AddOptions<TOptions>();

        if (configure is not null)
        {
            options.Configure(configure);
        }

        options.ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IValidateOptions<TOptions>,
                MessageOptionsValidator<TOptions>>());
    }
}

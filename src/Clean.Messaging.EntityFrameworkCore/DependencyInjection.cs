using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.EntityFrameworkCore;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging<TDbContext>(
        this IServiceCollection services,
        Action<MessagingBuilder<TDbContext>> configure)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(configure);

        Clean.Messaging.DependencyInjection.AddMessaging(services);
        PersistenceServices.EnsurePersistence<TDbContext>(services);

        configure(new MessagingBuilder<TDbContext>(services));

        return services;
    }
}

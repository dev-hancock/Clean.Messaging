using Clean.Messaging.EntityFrameworkCore;
using Clean.Messaging.Outbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Clean.Messaging.DomainEvents.EntityFrameworkCore;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainEvents(
        this IServiceCollection services)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IOutboxCapture, DomainEventCapture>());

        return services;
    }

    public static MessagingBuilder<TDbContext> AddDomainEvents<TDbContext>(
        this MessagingBuilder<TDbContext> builder)
        where TDbContext : DbContext
    {
        builder.Services.AddDomainEvents();

        return builder;
    }

}

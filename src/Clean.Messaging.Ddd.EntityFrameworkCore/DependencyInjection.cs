using Clean.Messaging.Ddd;
using Clean.Messaging.EntityFrameworkCore;
using Clean.Messaging.Outbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Clean.Messaging.Ddd.EntityFrameworkCore;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainEventCapture(
        this IServiceCollection services)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IOutboxCapture, DomainEventCapture>());

        return services;
    }

    public static MessagingBuilder<TDbContext> AddDomainEventCapture<TDbContext>(
        this MessagingBuilder<TDbContext> builder)
        where TDbContext : DbContext
    {
        builder.Services.AddDomainEventCapture();

        return builder;
    }

    public static ModelBuilder AddDomainEventCapture(
        this ModelBuilder modelBuilder)
    {
        var aggregates = modelBuilder.Model
            .GetEntityTypes()
            .Where(entity =>
                typeof(AggregateRoot).IsAssignableFrom(entity.ClrType))
            .Select(entity => entity.ClrType)
            .ToArray();

        foreach (var aggregate in aggregates)
        {
            modelBuilder.Entity(aggregate)
                .Ignore(nameof(AggregateRoot.Events));
        }

        return modelBuilder;
    }
}

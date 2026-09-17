using Clean.Messaging.Outbox.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Outbox.EntityFrameworkCore;

internal sealed class OutboxValidation<TDbContext>(
    IServiceScopeFactory scopes)
    : IHostedService
    where TDbContext : DbContext
{
    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();

        var services = scope.ServiceProvider;

        var db = services
            .GetRequiredService<TDbContext>();

        if (!db.Database.IsRelational())
        {
            throw new InvalidOperationException(
                "Messaging requires a relational EF Core provider.");
        }

        if (db.Model.FindEntityType(
                typeof(OutboxEntry)) is null)
        {
            throw new InvalidOperationException(
                "Map the outbox with ModelBuilder.AddOutbox().");
        }

        ValidateInterceptors(db);

        _ = services
            .GetRequiredService<OutboxProcessor>();

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static void ValidateInterceptors(
        DbContext db)
    {
        var interceptors = db
            .GetService<IDbContextOptions>()
            .FindExtension<CoreOptionsExtension>()
            ?.Interceptors;

        var hasSaveInterceptor = interceptors?
            .OfType<OutboxInterceptor>()
            .Any() == true;

        var hasTransactionInterceptor = interceptors?
            .OfType<OutboxTransactionInterceptor>()
            .Any() == true;

        if (!hasSaveInterceptor ||
            !hasTransactionInterceptor)
        {
            throw new InvalidOperationException(
                "Attach outbox tracking using DbContextOptionsBuilder.AddMessaging(services).");
        }
    }
}

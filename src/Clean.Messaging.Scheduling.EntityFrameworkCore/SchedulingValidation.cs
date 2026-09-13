using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Scheduling.EntityFrameworkCore;

internal sealed class SchedulingValidation<TDbContext>(
    IServiceScopeFactory scopes)
    : IHostedService
    where TDbContext : DbContext
{
    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();

        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<TDbContext>();

        if (!db.Database.IsRelational())
        {
            throw new InvalidOperationException(
                "Message scheduling requires a relational EF Core provider.");
        }

        if (db.Model.FindEntityType(
                typeof(ScheduledMessageEntry)) is null)
        {
            throw new InvalidOperationException(
                "Map scheduled message persistence with ModelBuilder.AddScheduling().");
        }

        ValidateInterceptors(db);

        _ = services.GetRequiredService<IScheduledMessageStore>();
        _ = services.GetRequiredService<IScheduledMessageExecutor>();

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
            .OfType<SchedulingSaveInterceptor>()
            .Any() == true;

        var hasTransactionInterceptor = interceptors?
            .OfType<SchedulingTransactionInterceptor>()
            .Any() == true;

        if (!hasSaveInterceptor ||
            !hasTransactionInterceptor)
        {
            throw new InvalidOperationException(
                "Attach scheduled message tracking using DbContextOptionsBuilder.AddScheduling<TDbContext>(services).");
        }
    }
}

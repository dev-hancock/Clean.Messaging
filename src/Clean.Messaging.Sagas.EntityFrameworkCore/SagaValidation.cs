using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Sagas.EntityFrameworkCore;

internal sealed class SagaValidation<TDbContext>(
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
                "Sagas require a relational EF Core provider.");
        }

        if (db.Model.FindEntityType(typeof(SagaEntry)) is null)
        {
            throw new InvalidOperationException(
                "Map saga persistence with ModelBuilder.AddSagas().");
        }

        ValidateInterceptor(db);

        _ = services.GetRequiredService<ISagaStore>();

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static void ValidateInterceptor(
        DbContext db)
    {
        var interceptors = db
            .GetService<IDbContextOptions>()
            .FindExtension<CoreOptionsExtension>()
            ?.Interceptors;

        if (interceptors?
                .OfType<SagaSaveInterceptor>()
                .Any() != true)
        {
            throw new InvalidOperationException(
                "Attach saga persistence using DbContextOptionsBuilder.AddSagas<TDbContext>(services).");
        }
    }
}

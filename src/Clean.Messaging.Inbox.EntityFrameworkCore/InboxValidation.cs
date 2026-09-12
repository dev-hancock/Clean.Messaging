using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Inbox.EntityFrameworkCore;

internal sealed class InboxValidation<TDbContext>(
    IServiceScopeFactory scopes)
    : IHostedService
    where TDbContext : DbContext
{
    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            scopes.CreateScope();

        var services =
            scope.ServiceProvider;

        var db = services
            .GetRequiredService<TDbContext>();

        if (!db.Database.IsRelational())
        {
            throw new InvalidOperationException(
                "Messaging requires a relational EF Core provider.");
        }

        if (db.Model.FindEntityType(
                typeof(InboxEntry)) is null ||
            db.Model.FindEntityType(
                typeof(InboxAdmission)) is null)
        {
            throw new InvalidOperationException(
                "Map the inbox with ModelBuilder.AddInbox().");
        }

        _ = services
            .GetRequiredService<InboxProcessor>();

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
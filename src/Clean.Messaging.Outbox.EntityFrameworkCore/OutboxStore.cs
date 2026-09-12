using Clean.Messaging.Outbox;
using Clean.Messaging.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Outbox.EntityFrameworkCore;

internal sealed class OutboxStore<TDbContext>(
    TDbContext db,
    IServiceScopeFactory scopes,
    IOutboxWriter writer)
    : DurableStore<TDbContext, OutboxEntry>(
        db,
        scopes),
      IOutboxStore
    where TDbContext : DbContext
{
    public void Stage(
        OutboxBatch batch)
    {
        writer.Stage(
            Db,
            batch);
    }
}

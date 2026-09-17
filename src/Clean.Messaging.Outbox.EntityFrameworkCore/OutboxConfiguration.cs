using Clean.Messaging.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Clean.Messaging.Outbox.Persistence;

namespace Clean.Messaging.Outbox.EntityFrameworkCore;

internal static class OutboxConfiguration
{
    public static void Configure(
        ModelBuilder modelBuilder)
    {
        DurableEntryConfiguration.Configure(
            modelBuilder.Entity<OutboxEntry>(),
            "Outbox");
    }
}

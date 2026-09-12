using Clean.Messaging.Outbox;
using Clean.Messaging.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

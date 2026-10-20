using Clean.Messaging.Inbox.EntityFrameworkCore;
using Clean.Messaging.Inbox.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clean.Messaging.EntityFrameworkCore.Tests.Infrastructure;

internal sealed class MessagingDbContext(DbContextOptions<MessagingDbContext> options) : DbContext(options)
{
    public DbSet<InboxEntry> Entries => Set<InboxEntry>();

    public DbSet<LocalEffect> Effects => Set<LocalEffect>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddInbox();
        modelBuilder.Entity<LocalEffect>().HasKey(effect => effect.Id);
    }
}

internal sealed record LocalEffect(Guid Id, string Value);

internal sealed record WorkItem(Guid Id, string Value);

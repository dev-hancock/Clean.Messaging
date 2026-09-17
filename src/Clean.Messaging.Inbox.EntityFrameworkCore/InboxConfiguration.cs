using Clean.Messaging.EntityFrameworkCore;
using Clean.Messaging.Inbox.Admission;
using Clean.Messaging.Inbox.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clean.Messaging.Inbox.EntityFrameworkCore;

internal static class InboxConfiguration
{
    public static void Configure(
        ModelBuilder modelBuilder)
    {
        ConfigureEntries(
            modelBuilder);

        ConfigureAdmissions(
            modelBuilder);
    }

    private static void ConfigureEntries(
        ModelBuilder modelBuilder)
    {
        DurableEntryConfiguration.Configure(
            modelBuilder.Entity<InboxEntry>(),
            "Inbox");
    }

    private static void ConfigureAdmissions(
        ModelBuilder modelBuilder)
    {
        var admission =
            modelBuilder.Entity<InboxAdmission>();

        admission.ToTable(
            "InboxAdmission");

        admission.HasKey(entry =>
            entry.MessageId);

        admission.HasIndex(entry =>
                entry.EventId)
            .IsUnique();

        admission.HasIndex(entry => new
        {
            entry.CreatedAt,
            entry.MessageId
        });

        admission.ComplexProperty(
            entry => entry.Message,
            message =>
            {
                message.Property(data => data.Type)
                    .HasMaxLength(500)
                    .IsRequired();

                message.Property(data => data.Value)
                    .IsRequired();
            });
    }
}
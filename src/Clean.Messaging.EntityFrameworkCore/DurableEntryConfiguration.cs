using Clean.Messaging.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clean.Messaging.EntityFrameworkCore;

public static class DurableEntryConfiguration
{
    public static void Configure<TEntry>(
        EntityTypeBuilder<TEntry> builder,
        string table)
        where TEntry : class, IDurableEntry
    {
        builder.ToTable(table);

        builder.HasKey(entry => new
        {
            entry.MessageId,
            entry.TargetId
        });

        builder.Property(entry => entry.TargetId)
            .HasMaxLength(200)
            .IsUnicode(false);

        builder.Property(entry => entry.ClaimId)
            .HasMaxLength(32)
            .IsUnicode(false);

        builder.Property(entry => entry.FailureCode)
            .HasMaxLength(200);

        builder.Property(entry => entry.ExceptionType)
            .HasMaxLength(500);

        builder.Property(entry => entry.FailureMessage)
            .HasMaxLength(500);

        builder.Ignore(entry => entry.Key);

        builder.ComplexProperty(entry => entry.Message, message =>
        {
            message.Property(data => data.Type)
                .HasMaxLength(500)
                .IsRequired();

            message.Property(data => data.Value)
                .IsRequired();
        });

        builder.HasIndex(entry => new
        {
            entry.EventId,
            entry.TargetId
        }).IsUnique();

        builder.HasIndex(entry => new
        {
            entry.ProcessedAt,
            entry.DeadLetteredAt,
            entry.DiscardedAt,
            entry.NextAttemptAt,
            entry.ClaimedUntil,
            entry.CreatedAt
        });

        builder.HasIndex(entry => new
        {
            entry.DiscardedAt,
            entry.DeadLetteredAt
        });
    }
}

using Clean.Messaging.Scheduling.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clean.Messaging.Scheduling.EntityFrameworkCore;

internal static class SchedulingConfiguration
{
    internal const string TableName = "ScheduledMessages";

    public static void Configure(
        ModelBuilder modelBuilder)
    {
        if (modelBuilder.Model.FindEntityType(
                typeof(ScheduleEntry)) is not null)
        {
            return;
        }

        var builder = modelBuilder.Entity<ScheduleEntry>();

        builder.ToTable(TableName);

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .HasConversion(
                id => id.Value,
                value => new(value));

        builder.Property(message => message.GroupId)
            .HasConversion(
                id => id.HasValue
                    ? id.Value.Value
                    : (Guid?)null,
                value => value.HasValue
                    ? new ScheduleGroupId(value.Value)
                    : null);

        builder.Property(message => message.Target)
            .HasConversion(
                target => target.Value,
                value => new(value))
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();

        builder.ComplexProperty(message => message.Message, data =>
        {
            data.Property(value => value.Type)
                .HasMaxLength(500)
                .IsRequired();

            data.Property(value => value.Value)
                .IsRequired();
        });

        builder.Property(message => message.ClaimId)
            .HasMaxLength(32)
            .IsUnicode(false);

        builder.Property(message => message.FailureCode)
            .HasMaxLength(200);

        builder.Property(message => message.FailureMessage)
            .HasMaxLength(500);

        builder.Property(message => message.ExceptionType)
            .HasMaxLength(500);

        builder.Ignore(message => message.IsTerminal);

        builder.Property(message => message.Version)
            .IsConcurrencyToken();

        builder.HasIndex(message => new
        {
            message.DispatchedAt,
            message.CancelledAt,
            message.FailedAt,
            message.NextAttemptAt,
            message.ClaimedUntil,
            message.DueAt
        });

        builder.HasIndex(message => new
        {
            message.GroupId,
            message.Target,
            message.DueAt
        });
    }
}

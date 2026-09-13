using Microsoft.EntityFrameworkCore;

namespace Clean.Messaging.Sagas.EntityFrameworkCore;

internal static class SagaConfiguration
{
    internal const string TableName = "Sagas";
    internal const string UniqueKeyIndexName = "UX_Sagas_Type_Key";

    public static void Configure(
        ModelBuilder modelBuilder)
    {
        if (modelBuilder.Model.FindEntityType(
                typeof(SagaEntry)) is not null)
        {
            return;
        }

        var builder = modelBuilder.Entity<SagaEntry>();

        builder.ToTable(TableName);

        builder.HasKey(saga => saga.Id);

        builder.Property(saga => saga.Id)
            .HasConversion(
                id => id.Value,
                value => new(value));

        builder.Property(saga => saga.Type)
            .HasMaxLength(200)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(saga => saga.Key)
            .HasConversion(
                key => key.Value,
                value => new(value))
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(saga => saga.State)
            .IsRequired();

        builder.Property(saga => saga.StatePurgedAt);

        builder.Property(saga => saga.Version)
            .IsConcurrencyToken();

        builder.Property(saga => saga.FailureCode)
            .HasMaxLength(200);

        builder.Property(saga => saga.FailureMessage)
            .HasMaxLength(500);

        builder.Ignore(saga => saga.IsActive);
        builder.Ignore(saga => saga.IsTerminal);

        builder.HasIndex(saga => new
        {
            saga.Type,
            saga.Key
        })
            .IsUnique()
            .HasDatabaseName(UniqueKeyIndexName);

        builder.HasIndex(saga => new
        {
            saga.StatePurgedAt,
            saga.CompletedAt,
            saga.FailedAt,
            saga.UpdatedAt
        });
    }
}

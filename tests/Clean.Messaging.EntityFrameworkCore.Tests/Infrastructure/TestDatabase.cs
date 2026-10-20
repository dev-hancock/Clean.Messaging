using Clean.Messaging.Inbox.EntityFrameworkCore;
using Clean.Messaging.Inbox.Persistence;
using Clean.Messaging.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Clean.Messaging.EntityFrameworkCore.Tests.Infrastructure;

internal sealed class TestDatabase : IAsyncDisposable
{
    public static DateTime Now { get; } = new(2030, 4, 5, 12, 0, 0, DateTimeKind.Utc);
    public static TimeSpan Lease { get; } = TimeSpan.FromMinutes(2);

    private readonly DbContextOptions<MessagingDbContext> _options;

    private TestDatabase(string connectionString, Action<IServiceCollection>? configure)
    {
        _options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(new FixedClock());
        services.AddDbContext<MessagingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddInbox<MessagingDbContext>();
        services.AddContract<WorkItem>("tests.work-item");
        configure?.Invoke(services);
        Services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    public ServiceProvider Services { get; }

    public MessagingDbContext CreateContext() => new(_options);

    public AsyncServiceScope CreateScope() => Services.CreateAsyncScope();

    public InboxEntry CreateEntry(string targetId, Guid? eventId = null, Guid? messageId = null)
    {
        var id = messageId ?? Guid.NewGuid();
        return new InboxEntry
        {
            MessageId = id,
            EventId = eventId ?? Guid.NewGuid(),
            TargetId = targetId,
            Message = Services.GetRequiredService<IMessageSerializer>().Serialize(new WorkItem(id, "durable work")),
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            CreatedAt = Now.AddDays(-1)
        };
    }

    public async Task Save(params InboxEntry[] entries)
    {
        await using var context = CreateContext();
        context.Entries.AddRange(entries);
        await context.SaveChangesAsync();
    }

    public async Task<InboxEntry[]> ReadEntries()
    {
        await using var context = CreateContext();
        return await context.Entries.AsNoTracking().ToArrayAsync();
    }

    internal static async Task<TestDatabase> Create(string serverConnectionString, Action<IServiceCollection>? configure)
    {
        var connection = new NpgsqlConnectionStringBuilder(serverConnectionString)
        {
            Database = $"messaging_{Guid.NewGuid():N}",
            Pooling = false,
            Timeout = 15,
            CommandTimeout = 20
        };
        var database = new TestDatabase(connection.ConnectionString, configure);
        try
        {
            await using var context = database.CreateContext();
            await context.Database.EnsureCreatedAsync();
            return database;
        }
        catch (Exception initializationFailure)
        {
            try
            {
                await database.DisposeAsync();
            }
            catch (Exception cleanupFailure)
            {
                initializationFailure.Data["TestDatabaseCleanupFailure"] = cleanupFailure;
            }
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await Services.DisposeAsync();
        }
        finally
        {
            await using var context = CreateContext();
            await context.Database.EnsureDeletedAsync();
        }
    }

    private sealed class FixedClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(Now);
    }
}

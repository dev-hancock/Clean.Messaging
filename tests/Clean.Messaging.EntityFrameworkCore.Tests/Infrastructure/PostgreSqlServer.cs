using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace Clean.Messaging.EntityFrameworkCore.Tests.Infrastructure;

public sealed class PostgreSqlServer : IAsyncInitializer, IAsyncDisposable
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17.7-alpine")
        .Build();

    public Task InitializeAsync() => _container.StartAsync();

    internal Task<TestDatabase> CreateDatabase(
        Action<Microsoft.Extensions.DependencyInjection.IServiceCollection>? configure = null)
    {
        return TestDatabase.Create(_container.GetConnectionString(), configure);
    }

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}

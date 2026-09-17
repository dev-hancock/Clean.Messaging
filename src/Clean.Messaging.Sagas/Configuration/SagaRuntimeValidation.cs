using Clean.Messaging.Outbox;
using Clean.Messaging.Sagas.Definition;
using Clean.Messaging.Sagas.Delivery;
using Clean.Messaging.Sagas.Persistence;
using Clean.Messaging.Scheduling.Delivery;
using Clean.Messaging.Scheduling.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Sagas.Configuration;

internal sealed class SagaRuntimeValidation(
    IServiceScopeFactory scopes)
    : IHostedService
{
    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();

        var services = scope.ServiceProvider;

        _ = services.GetRequiredService<IOutbox>();
        _ = services.GetRequiredService<IScheduleWriter>();
        _ = services.GetRequiredService<ISagaStore>();
        _ = services.GetRequiredService<SagaRegistry>();

        _ = services
            .GetServices<IScheduleDelivery>()
            .Single(delivery =>
                delivery.Target == SagaScheduleDelivery.Target);

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
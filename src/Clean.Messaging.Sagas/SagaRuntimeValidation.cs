using Clean.Messaging.Outbox;
using Clean.Messaging.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Sagas;

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
        _ = services.GetRequiredService<IScheduledMessageWriter>();
        _ = services.GetRequiredService<ISagaStore>();
        _ = services.GetRequiredService<SagaRegistry>();
        _ = services.GetRequiredService<ScheduledMessageDeliveryRegistry>();

        var deliveries = services
            .GetRequiredService<ScheduledMessageDeliveryRegistry>();

        _ = deliveries.Get(
            SagaScheduledMessageDelivery.Target);

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

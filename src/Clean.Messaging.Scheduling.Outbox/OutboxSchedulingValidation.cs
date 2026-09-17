using Clean.Messaging.Outbox;
using Clean.Messaging.Scheduling.Delivery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Scheduling.Outbox;

internal sealed class OutboxSchedulingValidation(
    IServiceScopeFactory scopes)
    : IHostedService
{
    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();

        var services = scope.ServiceProvider;

        _ = services.GetRequiredService<IOutbox>();
        _ = services.GetRequiredService<IMessageScheduler>();

        _ = services
            .GetServices<IScheduleDelivery>()
            .Single(delivery =>
                delivery.Target == ScheduleTarget.Message);

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

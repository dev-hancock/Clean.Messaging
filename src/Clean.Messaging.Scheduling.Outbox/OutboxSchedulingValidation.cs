using Clean.Messaging.Outbox;
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
            .GetServices<IScheduledMessageDelivery>()
            .Single(delivery =>
                delivery.Target == ScheduledMessageTarget.Message);

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Scheduling;

internal sealed class SchedulingRuntimeValidation(
    IServiceScopeFactory scopes)
    : IHostedService
{
    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();

        _ = scope.ServiceProvider.GetRequiredService<
            ScheduledMessageDeliveryRegistry>();

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

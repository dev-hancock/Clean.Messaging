using Clean.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Clean.Messaging.Consumers;

internal sealed class ConsumerValidation(
    IConsumerRegistry consumers,
    IMessageContractRegistry contracts,
    IServiceProviderIsService services) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var consumer in consumers.All)
        {
            _ = contracts.GetContract(consumer.MessageType);

            if (!services.IsService(consumer.ConsumerType))
            {
                throw new InvalidOperationException(
                    $"Consumer '{consumer.ConsumerId}' has no service registration.");
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
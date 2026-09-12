using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MQTTnet;

namespace Clean.Messaging.Outbox.Mqtt;

public static class DependencyInjection
{
    public static IServiceCollection AddMqttOutbox(
        this IServiceCollection services,
        MqttClientOptions clientOptions,
        Action<MqttOutboxOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(
            services);

        ArgumentNullException.ThrowIfNull(
            clientOptions);

        var options =
            services.AddOptions<MqttOutboxOptions>();

        if (configure is not null)
        {
            options.Configure(
                configure);
        }

        services.TryAddSingleton(
            clientOptions);

        services.TryAddSingleton<
            MqttConnection>();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IOutboxTargetProvider,
                MqttOutboxTargets>());

        services.AddOutboxTransport<
            MqttOutboxTransport>();

        return services;
    }
}
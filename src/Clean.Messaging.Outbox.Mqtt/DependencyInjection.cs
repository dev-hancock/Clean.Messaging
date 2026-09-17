using Clean.Messaging.Outbox.Mqtt.Connection;
using Clean.Messaging.Outbox.Mqtt.Publishing;
using Clean.Messaging.Outbox.Mqtt.Routing;
using Clean.Messaging.Outbox.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Outbox.Mqtt;

public static class DependencyInjection
{
    public static IServiceCollection AddMqttOutbox(
        this IServiceCollection services,
        Action<MqttOutboxOptions> configure,
        Action<MqttOutboxBuilder> routes)
    {
        ArgumentNullException.ThrowIfNull(configure);
        ArgumentNullException.ThrowIfNull(routes);

        services
            .AddOptions<MqttOutboxOptions>()
            .Configure(configure)
            .ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IValidateOptions<MqttOutboxOptions>,
                MqttOutboxOptionsValidator>());

        var builder =
            new MqttOutboxBuilder(services);

        routes(builder);

        if (builder.RouteCount == 0)
        {
            throw new InvalidOperationException(
                "At least one MQTT outbox route must be registered.");
        }

        services.TryAddSingleton<MqttConnection>();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IOutboxTargetProvider,
                MqttOutboxTargets>());

        services.TryAddSingleton<MqttRouteRegistry>();

        services.AddOutboxTransport<
            MqttOutboxTransport>();

        return services;
    }
}
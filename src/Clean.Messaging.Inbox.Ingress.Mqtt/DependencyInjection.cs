using Clean.Messaging.Inbox.Ingress.Mqtt.Configuration;
using Clean.Messaging.Inbox.Ingress.Mqtt.Routing;
using Clean.Messaging.Inbox.Ingress.Mqtt.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Inbox.Ingress.Mqtt;

public static class DependencyInjection
{
    public static IServiceCollection AddMqttInbox(
        this IServiceCollection services,
        Action<MqttInboxOptions> configure,
        Action<MqttInboxBuilder> routes)
    {
        ArgumentNullException.ThrowIfNull(
            services);

        ArgumentNullException.ThrowIfNull(
            configure);

        ArgumentNullException.ThrowIfNull(
            routes);

        services.AddIngress();

        services
            .AddOptions<MqttInboxOptions>()
            .Configure(configure)
            .ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IValidateOptions<MqttInboxOptions>,
                MqttInboxOptionsValidator>());

        var builder =
            new MqttInboxBuilder(
                services);

        routes(builder);

        if (builder.Routes == 0)
        {
            throw new InvalidOperationException(
                "At least one MQTT inbox route must be registered.");
        }

        services.TryAddSingleton<
            MqttInboxRouter>();

        services.TryAddScoped<
            MqttInboxIngressHandler>();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                MqttInboxWorker>());

        return services;
    }
}
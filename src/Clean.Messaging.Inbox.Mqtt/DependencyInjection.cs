using Clean.Messaging.Inbox.Mqtt.Configuration;
using Clean.Messaging.Inbox.Mqtt.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Inbox.Mqtt;

public static class DependencyInjection
{
    public static IServiceCollection AddMqttInbox(
        this IServiceCollection services,
        Action<MqttInboxOptions> configure,
        Action<MqttInboxBuilder> routes)
    {
        ArgumentNullException.ThrowIfNull(configure);
        ArgumentNullException.ThrowIfNull(routes);

        services
            .AddOptions<MqttInboxOptions>()
            .Configure(configure)
            .ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IValidateOptions<MqttInboxOptions>,
                MqttInboxOptionsValidator>());

        var builder = new MqttInboxBuilder(services);

        routes(builder);

        if (builder.RouteCount == 0)
        {
            throw new InvalidOperationException(
                "At least one MQTT inbox route must be registered.");
        }

        services.TryAddSingleton<MqttInboxRouter>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                Runtime.MqttInboxWorker>());

        return services;
    }
}

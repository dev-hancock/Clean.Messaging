using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            .Configure(configure);

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
                Microsoft.Extensions.Hosting.IHostedService,
                MqttInboxWorker>());

        return services;
    }
}

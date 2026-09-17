using Clean.Messaging.Sagas.Configuration;
using Clean.Messaging.Sagas.Definition;
using Clean.Messaging.Sagas.Delivery;
using Clean.Messaging.Sagas.Effects;
using Clean.Messaging.Sagas.Runtime;
using Clean.Messaging.Scheduling;
using Clean.Messaging.Scheduling.Delivery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Sagas;

public static class DependencyInjection
{
    public static IServiceCollection AddSagas(
        this IServiceCollection services,
        Action<SagaBuilder> configure,
        Action<SagaOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.AddMessaging();
        services.AddScheduling();

        var options = services.AddOptions<SagaOptions>();

        if (configureOptions is not null)
        {
            options.Configure(configureOptions);
        }

        options.ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IValidateOptions<SagaOptions>,
                SagaOptionsValidator>());

        services.TryAddSingleton<SagaStateSerializer>();
        services.TryAddSingleton<SagaRegistry>();

        services.TryAddScoped<SagaAttempt>();
        services.TryAddScoped<SagaEffectWriter>();
        services.TryAddScoped<SagaRuntime>();
        services.TryAddScoped<SagaProcessor>();
        services.TryAddScoped<ISagaManager, SagaManager>();

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<
                IScheduleDelivery,
                SagaScheduleDelivery>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                SagaRuntimeValidation>());


        var registrations =
            new SagaBuilder(services);

        configure(registrations);

        if (registrations.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one saga must be registered.");
        }

        return services;
    }
}
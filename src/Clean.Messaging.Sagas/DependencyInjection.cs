using Clean.Messaging.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Sagas;

public static class DependencyInjection
{
    public static IServiceCollection AddSagas(
        this IServiceCollection services,
        Action<SagaRegistrationBuilder> configure,
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
        services.TryAddScoped<SagaExecution>();
        services.TryAddScoped<SagaProcessor>();
        services.TryAddScoped<ISagaManager, SagaManager>();

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<
                IScheduledMessageDelivery,
                SagaScheduledMessageDelivery>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                SagaRuntimeValidation>());


        var registrations =
            new SagaRegistrationBuilder(services);

        configure(registrations);

        if (registrations.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one saga must be registered.");
        }

        return services;
    }
}

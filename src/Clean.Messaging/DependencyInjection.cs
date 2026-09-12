using Clean.Messaging.Abstractions;
using Clean.Messaging.Consumers;
using Clean.Messaging.Failures;
using Clean.Messaging.Processing;
using Clean.Messaging.Retry;
using Clean.Messaging.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Clean.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        Action<RetryOptions>? configure = null)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.TryAddSingleton<MessageContext>();
        services.TryAddSingleton<IMessageContext>(provider =>
            provider.GetRequiredService<MessageContext>());

        services.TryAddSingleton<IConsumerRegistry, ConsumerRegistry>();
        services.TryAddSingleton<IMessageContractRegistry, MessageContractRegistry>();
        services.TryAddSingleton<IMessageSerializer, MessageSerializer>();
        services.TryAddSingleton<MessageFailureFactory>();
        services.TryAddSingleton<RetryPolicy>();
        services.TryAddSingleton<ClaimRenewal>();
        services.TryAddScoped<ConsumerDispatcher>();

        var options = services.AddOptions<RetryOptions>();

        if (configure is not null)
        {
            options.Configure(configure);
        }

        options.ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IValidateOptions<RetryOptions>,
                RetryOptionsValidator>());

        services.AddHostedService<ConsumerValidation>();

        return services;
    }

    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddMessaging(options =>
            configuration
                .GetSection(RetryOptions.Section)
                .Bind(options));
    }

    public static IServiceCollection AddContract<TMessage>(
        this IServiceCollection services,
        string contract)
        where TMessage : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contract);

        if (contract.Length > 500)
        {
            throw new ArgumentException(
                "Contract identifiers cannot exceed 500 characters.",
                nameof(contract));
        }

        services.AddMessaging();

        services.AddSingleton(
            new MessageContractRegistration(
                typeof(TMessage),
                contract));

        return services;
    }

    public static IServiceCollection AddConsumer<TMessage, TConsumer>(
        this IServiceCollection services,
        string consumerId)
        where TMessage : notnull
        where TConsumer : class, IMessageConsumer<TMessage>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerId);

        if (consumerId.Length > 200 ||
            consumerId.Any(character => character > 127))
        {
            throw new ArgumentException(
                "Consumer identities must be at most 200 ASCII characters.",
                nameof(consumerId));
        }

        services.AddMessaging();
        services.TryAddScoped<TConsumer>();

        services.AddSingleton<IConsumerInvoker>(
            new ConsumerInvoker<TMessage, TConsumer>(consumerId));

        return services;
    }
}

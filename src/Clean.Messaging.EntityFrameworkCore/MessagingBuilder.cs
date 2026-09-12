using Clean.Messaging.Abstractions;
using Clean.Messaging.Retry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.EntityFrameworkCore;

public sealed class MessagingBuilder<TDbContext>
    where TDbContext : DbContext
{
    internal MessagingBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }

    public MessagingBuilder<TDbContext> AddContract<TMessage>(
        string contract)
        where TMessage : notnull
    {
        Clean.Messaging.DependencyInjection.AddContract<TMessage>(
            Services,
            contract);

        return this;
    }

    public MessagingBuilder<TDbContext> AddConsumer<TMessage, TConsumer>(
        string consumerId)
        where TMessage : notnull
        where TConsumer : class, IMessageConsumer<TMessage>
    {
        Clean.Messaging.DependencyInjection.AddConsumer<TMessage, TConsumer>(
            Services,
            consumerId);

        return this;
    }

    public MessagingBuilder<TDbContext> ConfigureRetry(
        Action<RetryOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        Services.Configure(configure);

        return this;
    }
}

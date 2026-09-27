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
        Services.AddContract<TMessage>(contract);

        return this;
    }

    public MessagingBuilder<TDbContext> AddConsumer<TMessage, TConsumer>(
        string consumerId)
        where TMessage : notnull
        where TConsumer : class, IMessageConsumer<TMessage>
    {
        Services.AddConsumer<TMessage, TConsumer>(consumerId);

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

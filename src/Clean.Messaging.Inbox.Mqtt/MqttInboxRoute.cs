using Microsoft.Extensions.DependencyInjection;
using MQTTnet;

namespace Clean.Messaging.Inbox.Mqtt;

internal sealed class MqttInboxRoute<TMessage>(
    string topicFilter,
    Func<MqttApplicationMessage, TMessage> deserialize,
    Func<MqttApplicationMessage, TMessage, InboxMessage<TMessage>> map)
    : IMqttInboxRoute
    where TMessage : notnull
{
    public string TopicFilter { get; } = topicFilter;

    public async ValueTask Accept(
        IServiceProvider services,
        MqttApplicationMessage message,
        CancellationToken cancellationToken)
    {
        InboxMessage<TMessage> incoming;

        try
        {
            var value = deserialize(message);

            incoming = map(
                message,
                value);

            if (incoming.Id == Guid.Empty)
            {
                throw new InvalidOperationException(
                    $"MQTT route '{TopicFilter}' produced an empty message ID.");
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new MqttInboxPoisonMessageException(
                $"MQTT message for route '{TopicFilter}' could not be read.",
                exception);
        }

        var inbox = services.GetRequiredService<IInbox>();

        await inbox.Accept(
            incoming,
            cancellationToken);
    }
}

internal sealed class MqttInboxPoisonMessageException
    : Exception
{
    public MqttInboxPoisonMessageException(
        string message)
        : base(message)
    {
    }

    public MqttInboxPoisonMessageException(
        string message,
        Exception innerException)
        : base(
            message,
            innerException)
    {
    }
}

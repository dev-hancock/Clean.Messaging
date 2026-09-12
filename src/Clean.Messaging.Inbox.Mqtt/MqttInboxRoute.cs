using Microsoft.Extensions.DependencyInjection;
using MQTTnet;

namespace Clean.Messaging.Inbox.Mqtt;

internal sealed class MqttInboxRoute<TMessage>(
    string topicFilter,
    Func<MqttApplicationMessage, TMessage> deserialize,
    Func<MqttApplicationMessage, TMessage, Guid> messageId)
    : IMqttInboxRoute
    where TMessage : notnull
{
    public string TopicFilter { get; } = topicFilter;

    public Func<MqttApplicationMessage, TMessage, Guid?>? EventId { get; set; }

    public Func<MqttApplicationMessage, TMessage, Guid?>? CorrelationId { get; set; }

    public Func<MqttApplicationMessage, TMessage, Guid?>? CausationId { get; set; }

    public async ValueTask Accept(
        IServiceProvider services,
        MqttApplicationMessage message,
        CancellationToken cancellationToken)
    {
        InboxMessage<TMessage> incoming;

        try
        {
            var value = deserialize(message);
            var id = messageId(message, value);

            if (id == Guid.Empty)
            {
                throw new InvalidOperationException(
                    $"MQTT route '{TopicFilter}' produced an empty message ID.");
            }

            incoming = new(
                id,
                value,
                CorrelationId?.Invoke(message, value),
                CausationId?.Invoke(message, value),
                EventId?.Invoke(message, value));
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new MqttInboxPoisonMessageException(
                TopicFilter,
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
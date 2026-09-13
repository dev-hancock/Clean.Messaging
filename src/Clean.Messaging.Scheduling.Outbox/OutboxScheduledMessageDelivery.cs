using Clean.Messaging.Outbox;
using Clean.Messaging.Serialization;
using System.Collections.Concurrent;
using System.Reflection;

namespace Clean.Messaging.Scheduling.Outbox;

internal sealed class OutboxScheduledMessageDelivery(
    IOutbox outbox,
    IMessageSerializer serializer)
    : IScheduledMessageDelivery
{
    private delegate void Enqueue(
        IOutbox outbox,
        ScheduledMessageEntry scheduled,
        object message);

    private static readonly ConcurrentDictionary<Type, Enqueue> Invokers = new();

    public ScheduledMessageTarget Target =>
        ScheduledMessageTarget.Message;

    public ValueTask Dispatch(
        ScheduledMessageEntry scheduled,
        CancellationToken cancellationToken)
    {
        var message = serializer.Deserialize(
            scheduled.Message);

        var enqueue = Invokers.GetOrAdd(
            message.GetType(),
            CreateInvoker);

        enqueue(
            outbox,
            scheduled,
            message);

        return ValueTask.CompletedTask;
    }

    private static Enqueue CreateInvoker(
        Type messageType)
    {
        var method = typeof(OutboxScheduledMessageDelivery)
            .GetMethod(
                nameof(EnqueueMessage),
                BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                "Scheduled outbox enqueue method was not found.");

        var closed = method.MakeGenericMethod(
            messageType);

        return (Enqueue)closed.CreateDelegate(
            typeof(Enqueue));
    }

    private static void EnqueueMessage<TMessage>(
        IOutbox outbox,
        ScheduledMessageEntry scheduled,
        object message)
        where TMessage : notnull
    {
        outbox.Enqueue(
            new OutboxMessage<TMessage>(
                scheduled.Id.Value,
                (TMessage)message,
                scheduled.CorrelationId,
                scheduled.CausationId));
    }
}

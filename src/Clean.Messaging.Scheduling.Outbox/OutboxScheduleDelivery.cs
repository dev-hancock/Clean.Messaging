using Clean.Messaging.Outbox;
using Clean.Messaging.Scheduling.Delivery;
using Clean.Messaging.Serialization;
using System.Collections.Concurrent;
using System.Reflection;

namespace Clean.Messaging.Scheduling.Outbox;

internal sealed class OutboxScheduleDelivery(
    IOutbox outbox,
    IMessageSerializer serializer)
    : IScheduleDelivery
{
    private delegate void Enqueue(
        IOutbox outbox,
        ScheduledDispatch scheduled,
        object message);

    private static readonly ConcurrentDictionary<Type, Enqueue> Invokers = new();

    public ScheduleTarget Target =>
        ScheduleTarget.Message;

    public ValueTask Dispatch(
        ScheduledDispatch scheduled,
        CancellationToken cancellationToken)
    {
        var message = serializer.Deserialize(
            scheduled.MessageData);

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
        var method = typeof(OutboxScheduleDelivery)
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
        ScheduledDispatch scheduled,
        object message)
        where TMessage : notnull
    {
        outbox.Enqueue(
            new OutboxMessage<TMessage>(
                scheduled.ScheduleId.Value,
                (TMessage)message,
                scheduled.CorrelationId,
                scheduled.CausationId));
    }
}

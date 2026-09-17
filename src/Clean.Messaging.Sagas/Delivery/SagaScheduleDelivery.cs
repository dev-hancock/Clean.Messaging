using Clean.Messaging.Sagas.Exceptions;
using Clean.Messaging.Sagas.Runtime;
using Clean.Messaging.Scheduling;
using Clean.Messaging.Scheduling.Delivery;

namespace Clean.Messaging.Sagas.Delivery;

internal sealed class SagaScheduleDelivery(
    SagaProcessor processor)
    : IScheduleDelivery
{
    internal static ScheduleTarget Target { get; } =
        new("saga");

    ScheduleTarget IScheduleDelivery.Target =>
        Target;

    public ValueTask Dispatch(
        ScheduledDispatch message,
        CancellationToken cancellationToken)
    {
        var groupId = message.ScheduleGroupId
                      ?? throw new SagaSerializationException(
                          $"Scheduled saga timer '{message.ScheduleId}' has no saga group.");

        return processor.Process(
            new(groupId.Value),
            message.MessageData,
            new(
                message.ScheduleId.Value,
                message.CorrelationId,
                message.CausationId,
                message.ScheduleId.Value),
            cancellationToken);
    }
}
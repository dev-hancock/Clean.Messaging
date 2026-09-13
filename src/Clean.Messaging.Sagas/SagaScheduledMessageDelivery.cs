using Clean.Messaging.Scheduling;

namespace Clean.Messaging.Sagas;

internal sealed class SagaScheduledMessageDelivery(
    SagaProcessor processor)
    : IScheduledMessageDelivery
{
    internal static ScheduledMessageTarget Target { get; } =
        new("saga");

    ScheduledMessageTarget IScheduledMessageDelivery.Target =>
        Target;

    public ValueTask Dispatch(
        ScheduledMessageDispatch message,
        CancellationToken cancellationToken)
    {
        var groupId = message.ScheduleGroupId
                      ?? throw new SagaSerializationException(
                          $"Scheduled saga timer '{message.ScheduleId}' has no saga group.");

        return processor.ProcessTimer(
            new SagaId(groupId.Value),
            message.MessageData,
            new(
                message.ScheduleId.Value,
                message.CorrelationId,
                message.CausationId,
                message.ScheduleId.Value),
            cancellationToken);
    }
}

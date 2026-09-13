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
        ScheduledMessageEntry message,
        CancellationToken cancellationToken)
    {
        var groupId = message.GroupId
                      ?? throw new SagaSerializationException(
                          $"Scheduled saga timer '{message.Id}' has no saga group.");

        return processor.ProcessTimer(
            new SagaId(groupId.Value),
            message.Message,
            new(
                message.Id.Value,
                message.CorrelationId,
                message.CausationId,
                message.Id.Value),
            cancellationToken);
    }
}

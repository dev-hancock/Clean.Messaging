using Clean.Messaging.Outbox;
using Clean.Messaging.Sagas.Delivery;
using Clean.Messaging.Sagas.Persistence;
using Clean.Messaging.Sagas.Runtime;
using Clean.Messaging.Scheduling;
using Clean.Messaging.Scheduling.Persistence;

namespace Clean.Messaging.Sagas.Effects;

internal sealed class SagaEffectWriter(
    IOutbox outbox,
    IScheduleWriter scheduler)
{
    public void Send<TMessage>(
        SagaMessage<TMessage> effect,
        SagaTrigger trigger)
        where TMessage : notnull
    {
        outbox.Enqueue(
            new OutboxMessage<TMessage>(
                effect.Id,
                effect.Message,
                effect.CorrelationId ?? trigger.CorrelationId,
                effect.CausationId ?? trigger.MessageId));
    }

    public void Schedule<TMessage>(
        SagaEntry saga,
        SagaSchedule<TMessage> effect,
        SagaTrigger trigger)
        where TMessage : notnull
    {
        scheduler.Schedule(
            new(effect.Id),
            effect.Message,
            effect.DueAt,
            SagaScheduleDelivery.Target,
            Group(saga.Id),
            trigger.CorrelationId,
            trigger.MessageId);
    }

    public ValueTask Cancel(
        SagaEntry saga,
        Guid timerId,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        if (timerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Saga timer ID cannot be empty.",
                nameof(timerId));
        }

        if (trigger.TimerId == timerId)
        {
            return ValueTask.CompletedTask;
        }

        return scheduler.Cancel(
            new(timerId),
            SagaScheduleDelivery.Target,
            Group(saga.Id),
            cancellationToken);
    }

    private static ScheduleGroupId Group(
        SagaId sagaId)
    {
        return new(sagaId.Value);
    }
}
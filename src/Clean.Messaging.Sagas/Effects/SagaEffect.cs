using Clean.Messaging.Sagas.Persistence;
using Clean.Messaging.Sagas.Runtime;

namespace Clean.Messaging.Sagas.Effects;

public abstract record SagaEffect
{
    internal abstract ValueTask Apply(
        SagaEffectWriter writer,
        SagaEntry saga,
        SagaTrigger trigger,
        CancellationToken cancellationToken);
}

public sealed record SagaMessage<TMessage>(
    Guid Id,
    TMessage Message,
    Guid? CorrelationId = null,
    Guid? CausationId = null)
    : SagaEffect
    where TMessage : notnull
{
    internal override ValueTask Apply(
        SagaEffectWriter writer,
        SagaEntry saga,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        writer.Send(
            this,
            trigger);

        return ValueTask.CompletedTask;
    }
}

public sealed record SagaSchedule<TMessage>(
    Guid Id,
    DateTimeOffset DueAt,
    TMessage Message)
    : SagaEffect
    where TMessage : notnull
{
    internal override ValueTask Apply(
        SagaEffectWriter writer,
        SagaEntry saga,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        writer.Schedule(
            saga,
            this,
            trigger);

        return ValueTask.CompletedTask;
    }
}

public sealed record SagaCancel(Guid TimerId)
    : SagaEffect
{
    internal override ValueTask Apply(
        SagaEffectWriter writer,
        SagaEntry saga,
        SagaTrigger trigger,
        CancellationToken cancellationToken)
    {
        return writer.Cancel(
            saga,
            TimerId,
            trigger,
            cancellationToken);
    }
}
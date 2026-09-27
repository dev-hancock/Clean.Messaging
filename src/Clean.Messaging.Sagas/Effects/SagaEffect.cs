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
public sealed record SagaMessage<TMessage> : SagaEffect
    where TMessage : notnull
{
    public SagaMessage(
        Guid id,
        TMessage message,
        Guid? correlationId = null,
        Guid? causationId = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Saga message ID cannot be empty.",
                nameof(id));
        }

        ArgumentNullException.ThrowIfNull(message);

        Id = id;
        Message = message;
        CorrelationId = correlationId;
        CausationId = causationId;
    }

    public Guid Id { get; }

    public TMessage Message { get; }

    public Guid? CorrelationId { get; }

    public Guid? CausationId { get; }

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
public sealed record SagaSchedule<TMessage> : SagaEffect
    where TMessage : notnull
{
    public SagaSchedule(
        Guid id,
        DateTimeOffset dueAt,
        TMessage message)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Saga timer ID cannot be empty.",
                nameof(id));
        }

        ArgumentNullException.ThrowIfNull(message);

        Id = id;
        DueAt = dueAt;
        Message = message;
    }

    public Guid Id { get; }

    public DateTimeOffset DueAt { get; }

    public TMessage Message { get; }

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
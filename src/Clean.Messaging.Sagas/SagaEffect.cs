namespace Clean.Messaging.Sagas;

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

public static class SagaEffects
{
    public static SagaMessage<TMessage> Send<TMessage>(
        TMessage message)
        where TMessage : notnull
    {
        ArgumentNullException.ThrowIfNull(message);

        return new(
            Guid.NewGuid(),
            message);
    }

    public static SagaMessage<TMessage> Send<TMessage>(
        Guid id,
        TMessage message)
        where TMessage : notnull
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Saga message ID cannot be empty.",
                nameof(id));
        }

        ArgumentNullException.ThrowIfNull(message);

        return new(
            id,
            message);
    }

    public static SagaSchedule<TMessage> Schedule<TMessage>(
        DateTimeOffset dueAt,
        TMessage message)
        where TMessage : notnull
    {
        ArgumentNullException.ThrowIfNull(message);

        return new(
            Guid.NewGuid(),
            dueAt,
            message);
    }

    public static SagaSchedule<TMessage> Schedule<TMessage>(
        Guid timerId,
        DateTimeOffset dueAt,
        TMessage message)
        where TMessage : notnull
    {
        if (timerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Saga timer ID cannot be empty.",
                nameof(timerId));
        }

        ArgumentNullException.ThrowIfNull(message);

        return new(
            timerId,
            dueAt,
            message);
    }

    public static SagaCancel Cancel(
        Guid timerId)
    {
        if (timerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Saga timer ID cannot be empty.",
                nameof(timerId));
        }

        return new(timerId);
    }
}

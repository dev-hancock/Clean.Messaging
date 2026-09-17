using Clean.Messaging.Sagas.Effects;
using Clean.Messaging.Sagas.Persistence;
using System.Collections.Immutable;

namespace Clean.Messaging.Sagas;

internal sealed record SagaContinued(
    ImmutableArray<SagaEffect> Effects)
    : SagaResult(Effects)
{
    internal override void Apply(
        SagaEntry saga,
        DateTime now)
    {
    }

    protected override SagaResult WithEffects(
        ImmutableArray<SagaEffect> effects)
    {
        return new SagaContinued(effects);
    }
}

internal sealed record SagaCompleted(
    ImmutableArray<SagaEffect> Effects)
    : SagaResult(Effects)
{
    internal override void Apply(
        SagaEntry saga,
        DateTime now)
    {
        saga.Complete(now);
    }

    protected override SagaResult WithEffects(
        ImmutableArray<SagaEffect> effects)
    {
        return new SagaCompleted(effects);
    }
}

internal sealed record SagaFailed(
    string Code,
    string Message,
    ImmutableArray<SagaEffect> Effects)
    : SagaResult(Effects)
{
    protected override SagaResult WithEffects(
        ImmutableArray<SagaEffect> effects)
    {
        return new SagaFailed(
            Code,
            Message,
            effects);
    }

    internal override void Apply(
        SagaEntry saga,
        DateTime now)
    {
        saga.Fail(
            Code,
            Message,
            now);
    }
}

public abstract record SagaResult
{
    private protected SagaResult(
        ImmutableArray<SagaEffect> effects)
    {
        Effects = effects;
    }

    internal ImmutableArray<SagaEffect> Effects { get; }

    public static implicit operator ValueTask<SagaResult>(SagaResult result)
    {
        return ValueTask.FromResult(result);
    }

    public static SagaResult Continue()
    {
        return new SagaContinued([]);
    }

    public static SagaResult Complete()
    {
        return new SagaCompleted([]);
    }

    public static SagaResult Fail(
        string code,
        string message)
    {
        return new SagaFailed(
            code,
            message,
            []);
    }

    public SagaResult Send<TMessage>(
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

        return Add(
            new SagaMessage<TMessage>(
                id,
                message));
    }

    public SagaResult Schedule<TMessage>(
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

        return Add(
            new SagaSchedule<TMessage>(
                timerId,
                dueAt,
                message));
    }

    public SagaResult Cancel(
        Guid timerId)
    {
        if (timerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Saga timer ID cannot be empty.",
                nameof(timerId));
        }

        return Add(
            new SagaCancel(timerId));
    }

    protected abstract SagaResult WithEffects(
        ImmutableArray<SagaEffect> effects);

    internal abstract void Apply(
        SagaEntry saga,
        DateTime now);

    private SagaResult Add(
        SagaEffect effect)
    {
        return WithEffects(
            Effects.Add(effect));
    }
}
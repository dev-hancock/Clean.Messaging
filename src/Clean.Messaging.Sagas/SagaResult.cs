namespace Clean.Messaging.Sagas;

public abstract record SagaResult
{
    private SagaResult(
        IReadOnlyList<SagaEffect> effects)
    {
        ArgumentNullException.ThrowIfNull(effects);

        if (effects.Any(effect => effect is null))
        {
            throw new ArgumentException(
                "Saga effects cannot contain null values.",
                nameof(effects));
        }

        Effects = effects.ToArray();
    }

    public IReadOnlyList<SagaEffect> Effects { get; }

    public static SagaResult Continue(
        params SagaEffect[] effects)
    {
        return new Continuing(effects);
    }

    public static SagaResult Complete(
        params SagaEffect[] effects)
    {
        return new Completing(effects);
    }

    public static SagaResult Fail(
        string code,
        string message,
        params SagaEffect[] effects)
    {
        return new Failing(
            new(code, message),
            effects);
    }

    internal abstract void Apply(
        SagaEntry saga,
        DateTime now);

    private sealed record Continuing
        : SagaResult
    {
        public Continuing(
            IReadOnlyList<SagaEffect> effects)
            : base(effects)
        {
        }

        internal override void Apply(
            SagaEntry saga,
            DateTime now)
        {
        }
    }

    private sealed record Completing
        : SagaResult
    {
        public Completing(
            IReadOnlyList<SagaEffect> effects)
            : base(effects)
        {
        }

        internal override void Apply(
            SagaEntry saga,
            DateTime now)
        {
            saga.Complete(now);
        }
    }

    private sealed record Failing
        : SagaResult
    {
        public Failing(
            SagaFailure failure,
            IReadOnlyList<SagaEffect> effects)
            : base(effects)
        {
            Failure = failure;
        }

        public SagaFailure Failure { get; }

        internal override void Apply(
            SagaEntry saga,
            DateTime now)
        {
            saga.Fail(
                Failure.Code,
                Failure.Message,
                now);
        }
    }
}
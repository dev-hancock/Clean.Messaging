namespace Clean.Messaging.Sagas;

public readonly record struct SagaId(Guid Value)
{
    public static SagaId New()
    {
        return new(Guid.NewGuid());
    }

    public override string ToString()
    {
        return Value.ToString("D");
    }
}
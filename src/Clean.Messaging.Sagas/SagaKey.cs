namespace Clean.Messaging.Sagas;

public readonly record struct SagaKey
{
    private const int MaxLength = 500;

    public SagaKey(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"Saga keys cannot exceed {MaxLength} characters.");
        }

        Value = value;
    }

    public string Value { get; }

    public static SagaKey From(string value)
    {
        return new(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
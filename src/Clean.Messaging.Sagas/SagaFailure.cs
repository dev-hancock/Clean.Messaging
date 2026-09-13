namespace Clean.Messaging.Sagas;

public sealed record SagaFailure
{
    public SagaFailure(
        string code,
        string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (code.Length > 200)
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                "Saga failure codes cannot exceed 200 characters.");
        }

        if (message.Length > 500)
        {
            throw new ArgumentOutOfRangeException(
                nameof(message),
                "Saga failure messages cannot exceed 500 characters.");
        }

        Code = code;
        Message = message;
    }

    public string Code { get; }

    public string Message { get; }
}
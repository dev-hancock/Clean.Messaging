namespace Clean.Messaging.Sagas;

public sealed record Saga(
    SagaId Id,
    string Type,
    SagaKey Key,
    long Version,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? StatePurgedAt,
    DateTime? CompletedAt,
    DateTime? FailedAt,
    string? FailureCode,
    string? FailureMessage)
{
    public bool IsActive =>
        CompletedAt is null &&
        FailedAt is null;

    public bool IsTerminal => !IsActive;
}

namespace Clean.Messaging.Sagas;

public sealed record SagaTimer(
    Guid Id,
    SagaId SagaId,
    string SagaType,
    string MessageContract,
    DateTime DueAt,
    int Attempts,
    DateTime? NextAttemptAt,
    DateTime? ConsumedAt,
    DateTime? CancelledAt,
    DateTime? FailedAt,
    string? FailureCode,
    string? FailureMessage)
{
    public bool IsTerminal =>
        ConsumedAt is not null ||
        CancelledAt is not null ||
        FailedAt is not null;
}

namespace Clean.Messaging.Sagas.Persistence;

internal sealed class SagaEntry
{
    public const string TombstoneState = "null";

    public SagaId Id { get; init; }

    public required string Type { get; init; }

    public required SagaKey Key { get; init; }

    public required string State { get; set; }

    public long Version { get; set; }

    public Guid StartedByMessageId { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? StatePurgedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public DateTime? FailedAt { get; private set; }

    public string? FailureCode { get; private set; }

    public string? FailureMessage { get; private set; }

    public bool IsActive =>
        CompletedAt is null &&
        FailedAt is null;

    public bool IsTerminal => !IsActive;

    public void ApplyState(
        string state,
        long expectedVersion,
        DateTime now)
    {
        EnsureActive();

        if (Version != expectedVersion)
        {
            throw new InvalidOperationException(
                $"Saga '{Id}' changed from version {expectedVersion} to {Version} before its commit was applied.");
        }

        State = state;
        Version++;
        UpdatedAt = now;
    }

    public void Complete(
        DateTime now)
    {
        EnsureActive();

        CompletedAt = now;
        FailureCode = null;
        FailureMessage = null;
        UpdatedAt = now;
    }

    public void Fail(
        string code,
        string message,
        DateTime now)
    {
        EnsureActive();

        FailedAt = now;
        FailureCode = code;
        FailureMessage = message;
        UpdatedAt = now;
    }

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException(
                $"Saga '{Id}' is already terminal.");
        }
    }
}
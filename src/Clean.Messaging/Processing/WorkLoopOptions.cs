namespace Clean.Messaging.Processing;

internal sealed record WorkLoopOptions(
    int Capacity,
    int Concurrency,
    int BatchSize,
    TimeSpan Lease,
    TimeSpan RecoveryInterval,
    TimeSpan FailureMaxDelay)
{
    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            Capacity);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            Concurrency);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            BatchSize);

        if (Lease <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Lease));
        }

        if (RecoveryInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(RecoveryInterval));
        }

        if (FailureMaxDelay <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(FailureMaxDelay));
        }
    }
}

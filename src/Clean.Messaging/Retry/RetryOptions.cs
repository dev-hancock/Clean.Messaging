namespace Clean.Messaging.Retry;

public enum RetryBackoff
{
    Fixed,
    Linear,
    Exponential
}

public sealed class RetryOptions
{
    public const string Section = "Messaging:Retry";

    public int MaxAttempts { get; set; } = 5;

    public TimeSpan InitialDelay { get; set; } =
        TimeSpan.FromSeconds(2);

    public TimeSpan MaxDelay { get; set; } =
        TimeSpan.FromMinutes(1);

    public RetryBackoff Backoff { get; set; } =
        RetryBackoff.Exponential;

    public bool UseJitter { get; set; } = true;
}

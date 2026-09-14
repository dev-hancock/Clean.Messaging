namespace Clean.Messaging.Scheduling;

public sealed class SchedulingOptions
{
    public const string Section = "Messaging:Scheduling";

    public SchedulingLeaseOptions Lease { get; set; } = new();

    public SchedulingProcessingOptions Processing { get; set; } = new();

    public SchedulingRecoveryOptions Recovery { get; set; } = new();

    public SchedulingRetryOptions Retry { get; set; } = new();

    public SchedulingCleanupOptions Cleanup { get; set; } = new();
}

public sealed class SchedulingLeaseOptions
{
    public TimeSpan Duration { get; set; } =
        TimeSpan.FromMinutes(1);
}

public sealed class SchedulingProcessingOptions
{
    public int Concurrency { get; set; } = 4;
}

public sealed class SchedulingRecoveryOptions
{
    public TimeSpan Interval { get; set; } =
        TimeSpan.FromSeconds(30);

    public int BatchSize { get; set; } = 32;
}

public sealed class SchedulingRetryOptions
{
    public int MaxAttempts { get; set; } = 5;

    public TimeSpan Delay { get; set; } =
        TimeSpan.FromSeconds(2);

    public TimeSpan MaxDelay { get; set; } =
        TimeSpan.FromMinutes(1);
}

public sealed class SchedulingCleanupOptions
{
    public TimeSpan Retention { get; set; } =
        TimeSpan.FromDays(30);

    public TimeSpan Interval { get; set; } =
        TimeSpan.FromHours(1);

    public int BatchSize { get; set; } = 500;

    public int MaxBatchesPerRun { get; set; } = 20;
}

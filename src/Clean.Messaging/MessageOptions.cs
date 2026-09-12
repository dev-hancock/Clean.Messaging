namespace Clean.Messaging;

public class MessageOptions
{
    public MessageLeaseOptions Lease { get; set; } = new();

    public MessageProcessingOptions Processing { get; set; } = new();

    public MessageRecoveryOptions Recovery { get; set; } = new();

    public MessageCleanupOptions Cleanup { get; set; } = new();
}

public sealed class MessageLeaseOptions
{
    public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(30);

    public TimeSpan RenewalInterval { get; set; } = TimeSpan.FromSeconds(10);
}

public sealed class MessageProcessingOptions
{
    public int Concurrency { get; set; } = 4;

    public int Capacity { get; set; } = 1024;

    public int BatchSize { get; set; } = 100;

    public TimeSpan SettlementTimeout { get; set; } = TimeSpan.FromSeconds(5);
}

public sealed class MessageRecoveryOptions
{
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(10);

    public TimeSpan FailureMaxDelay { get; set; } = TimeSpan.FromMinutes(1);
}

public sealed class MessageCleanupOptions
{
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

    public TimeSpan Retention { get; set; } = TimeSpan.FromDays(7);

    public int BatchSize { get; set; } = 500;
}
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Processing;

public sealed class MessageOptionsValidator<TOptions> : IValidateOptions<TOptions>
    where TOptions : MessageOptions
{
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        Validate(options.Lease, failures);
        Validate(options.Processing, failures);
        Validate(options.Recovery, failures);
        Validate(options.Cleanup, failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void Validate(
        MessageLeaseOptions options,
        List<string> failures)
    {
        if (options.Duration <= TimeSpan.Zero)
        {
            failures.Add("Lease.Duration must be greater than zero.");
        }

        if (options.RenewalInterval <= TimeSpan.Zero)
        {
            failures.Add("Lease.RenewalInterval must be greater than zero.");
        }

        if (options.RenewalInterval >= options.Duration)
        {
            failures.Add("Lease.RenewalInterval must be less than Lease.Duration.");
        }
    }

    private static void Validate(
        MessageProcessingOptions options,
        List<string> failures)
    {
        if (options.Concurrency <= 0)
        {
            failures.Add("Processing.Concurrency must be greater than zero.");
        }

        if (options.Capacity <= 0)
        {
            failures.Add("Processing.Capacity must be greater than zero.");
        }

        if (options.BatchSize <= 0)
        {
            failures.Add("Processing.BatchSize must be greater than zero.");
        }

        if (options.SettlementTimeout <= TimeSpan.Zero)
        {
            failures.Add("Processing.SettlementTimeout must be greater than zero.");
        }
    }

    private static void Validate(
        MessageRecoveryOptions options,
        List<string> failures)
    {
        if (options.Interval <= TimeSpan.Zero)
        {
            failures.Add("Recovery.Interval must be greater than zero.");
        }

        if (options.FailureMaxDelay < options.Interval)
        {
            failures.Add("Recovery.FailureMaxDelay must be at least Recovery.Interval.");
        }
    }

    private static void Validate(
        MessageCleanupOptions options,
        List<string> failures)
    {
        if (options.Interval <= TimeSpan.Zero)
        {
            failures.Add("Cleanup.Interval must be greater than zero.");
        }

        if (options.Retention <= TimeSpan.Zero)
        {
            failures.Add("Cleanup.Retention must be greater than zero.");
        }

        if (options.BatchSize <= 0)
        {
            failures.Add("Cleanup.BatchSize must be greater than zero.");
        }
    }
}

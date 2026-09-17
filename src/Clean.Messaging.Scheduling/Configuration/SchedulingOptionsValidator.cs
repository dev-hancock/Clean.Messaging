using Microsoft.Extensions.Options;

namespace Clean.Messaging.Scheduling.Configuration;

internal sealed class SchedulingOptionsValidator
    : IValidateOptions<SchedulingOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        SchedulingOptions options)
    {
        var failures = new List<string>();

        if (options.Lease.Duration <= TimeSpan.Zero)
        {
            failures.Add("Scheduling lease duration must be greater than zero.");
        }

        if (options.Processing.Concurrency <= 0)
        {
            failures.Add("Scheduling concurrency must be greater than zero.");
        }

        if (options.Recovery.Interval <= TimeSpan.Zero)
        {
            failures.Add("Scheduling recovery interval must be greater than zero.");
        }

        if (options.Recovery.BatchSize <= 0)
        {
            failures.Add("Scheduling batch size must be greater than zero.");
        }

        if (options.Retry.MaxAttempts <= 0)
        {
            failures.Add("Scheduling maximum attempts must be greater than zero.");
        }

        if (options.Retry.Delay < TimeSpan.Zero)
        {
            failures.Add("Scheduling retry delay cannot be negative.");
        }

        if (options.Retry.MaxDelay < options.Retry.Delay)
        {
            failures.Add("Scheduling maximum retry delay cannot be shorter than the initial retry delay.");
        }

        if (options.Cleanup.Retention <= TimeSpan.Zero)
        {
            failures.Add("Scheduling retention must be greater than zero.");
        }

        if (options.Cleanup.Interval <= TimeSpan.Zero)
        {
            failures.Add("Scheduling cleanup interval must be greater than zero.");
        }

        if (options.Cleanup.BatchSize <= 0)
        {
            failures.Add("Scheduling cleanup batch size must be greater than zero.");
        }

        if (options.Cleanup.MaxBatchesPerRun <= 0)
        {
            failures.Add("Scheduling cleanup max batches per run must be greater than zero.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
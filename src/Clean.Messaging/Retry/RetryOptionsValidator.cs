using Microsoft.Extensions.Options;

namespace Clean.Messaging.Retry;

internal sealed class RetryOptionsValidator
    : IValidateOptions<RetryOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        RetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.MaxAttempts <= 0)
        {
            failures.Add(
                "Retry MaxAttempts must be greater than zero.");
        }

        if (options.InitialDelay < TimeSpan.Zero)
        {
            failures.Add(
                "Retry InitialDelay cannot be negative.");
        }

        if (options.MaxDelay < options.InitialDelay)
        {
            failures.Add(
                "Retry MaxDelay cannot be less than InitialDelay.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}

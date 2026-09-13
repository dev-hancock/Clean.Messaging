using Microsoft.Extensions.Options;

namespace Clean.Messaging.Sagas;

internal sealed class SagaOptionsValidator
    : IValidateOptions<SagaOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        SagaOptions options)
    {
        var failures = new List<string>();

        if (options.Retention <= TimeSpan.Zero)
        {
            failures.Add("Saga retention must be greater than zero.");
        }

        if (options.CleanupInterval <= TimeSpan.Zero)
        {
            failures.Add("Saga cleanup interval must be greater than zero.");
        }

        if (options.CleanupBatchSize <= 0)
        {
            failures.Add("Saga cleanup batch size must be greater than zero.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}

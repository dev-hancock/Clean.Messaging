using Clean.Messaging.Exceptions;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Retry;

internal readonly record struct RetryResult(
    bool ShouldRetry,
    TimeSpan Delay)
{
    public static RetryResult Fault =>
        new(false, TimeSpan.Zero);

    public static RetryResult After(
        TimeSpan delay) =>
        new(true, delay);
}

internal sealed class RetryPolicy(
    IOptions<RetryOptions> options)
{
    private readonly RetryOptions _options = options.Value;

    public RetryResult Evaluate(
        Exception exception,
        int attempt)
    {
        if (!CanRetry(exception, attempt))
        {
            return RetryResult.Fault;
        }

        var delay = GetDelay(attempt);

        if (_options.UseJitter)
        {
            delay = AddJitter(delay);
        }

        return RetryResult.After(delay);
    }

    private bool CanRetry(
        Exception exception,
        int attempt)
    {
        if (exception is MessageException { IsTransient: false })
        {
            return false;
        }

        return attempt < _options.MaxAttempts;
    }

    private TimeSpan GetDelay(int attempt)
    {
        var multiplier = _options.Backoff switch
        {
            RetryBackoff.Fixed => 1,
            RetryBackoff.Linear => attempt,
            RetryBackoff.Exponential => Math.Pow(
                2,
                Math.Max(0, attempt - 1)),

            _ => throw new ArgumentOutOfRangeException(
                nameof(_options.Backoff))
        };

        var milliseconds = Math.Min(
            _options.MaxDelay.TotalMilliseconds,
            _options.InitialDelay.TotalMilliseconds * multiplier);

        return TimeSpan.FromMilliseconds(milliseconds);
    }

    private static TimeSpan AddJitter(
        TimeSpan delay)
    {
        return TimeSpan.FromMilliseconds(
            Random.Shared.NextDouble() *
            delay.TotalMilliseconds);
    }
}

using System.Diagnostics.Metrics;

namespace Clean.Messaging.Processing;

internal static class MessageMetrics
{
    private static readonly Meter Meter = new("Clean.Messaging");

    private static readonly Counter<long> Completions =
        Meter.CreateCounter<long>("messaging.completed");

    private static readonly Counter<long> Retries =
        Meter.CreateCounter<long>("messaging.retries");

    private static readonly Counter<long> DeadLetters =
        Meter.CreateCounter<long>("messaging.dead_letters");

    private static readonly Counter<long> LeaseLosses =
        Meter.CreateCounter<long>("messaging.lease_losses");

    public static void Completed(string source) =>
        Completions.Add(1, Source(source));

    public static void Retried(string source) =>
        Retries.Add(1, Source(source));

    public static void DeadLettered(string source) =>
        DeadLetters.Add(1, Source(source));

    public static void LeaseLost(string source) =>
        LeaseLosses.Add(1, Source(source));

    private static KeyValuePair<string, object?> Source(string source) =>
        new("source", source);
}

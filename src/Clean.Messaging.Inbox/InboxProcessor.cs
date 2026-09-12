using Clean.Messaging.Failures;
using Clean.Messaging.Persistence;
using Clean.Messaging.Processing;
using Clean.Messaging.Retry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clean.Messaging.Inbox;

internal sealed partial class InboxProcessor(
    IInboxStore store,
    IInboxDispatcher dispatcher,
    ClaimRenewal renewal,
    MessageContext context,
    RetryPolicy retry,
    MessageFailureFactory failures,
    TimeProvider time,
    IOptions<InboxOptions> options,
    ILogger<InboxProcessor> logger)
    : IWorkProcessor<InboxEntry>
{
    private const string MetricName = "inbox";

    public async ValueTask Process(
        OwnedEntry<InboxEntry> owned,
        CancellationToken cancellationToken)
    {
        var entry = owned.Entry;
        var settings = options.Value;

        using var metadata = context.Push(
            entry.MessageId,
            entry.CorrelationId,
            entry.CausationId,
            entry.ConsumerId);

        using var claimLost = new CancellationTokenSource();

        using var handling =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                claimLost.Token);

        using var renewalCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        var renewalTask = renewal.Run(
            store,
            entry.Key,
            owned.ClaimId,
            settings.Lease,
            claimLost,
            renewalCancellation.Token);

        try
        {
            await Process(
                owned,
                claimLost,
                handling.Token,
                settings.Processing.SettlementTimeout,
                cancellationToken);
        }
        finally
        {
            await StopRenewal(
                renewalCancellation,
                renewalTask);
        }
    }

    private async Task Process(
        OwnedEntry<InboxEntry> owned,
        CancellationTokenSource claimLost,
        CancellationToken handlingToken,
        TimeSpan settlementTimeout,
        CancellationToken cancellationToken)
    {
        var outcome = await Attempt(
            owned,
            claimLost,
            handlingToken,
            cancellationToken);

        if (outcome.Completed)
        {
            Complete(owned.Entry);
            return;
        }

        if (claimLost.IsCancellationRequested)
        {
            ClaimLost(owned.Entry);
            return;
        }

        if (outcome.Failure is not null)
        {
            await Settle(
                owned,
                outcome.Failure,
                settlementTimeout,
                claimLost.Token);

            return;
        }

        ClaimLost(owned.Entry);
    }

    private async ValueTask<AttemptOutcome> Attempt(
        OwnedEntry<InboxEntry> owned,
        CancellationTokenSource claimLost,
        CancellationToken handlingToken,
        CancellationToken cancellationToken)
    {
        try
        {
            var completed = await dispatcher.Dispatch(
                owned,
                handlingToken);

            return new(
                completed,
                null);
        }
        catch (OperationCanceledException)
            when (claimLost.IsCancellationRequested &&
                  !cancellationToken.IsCancellationRequested)
        {
            return default;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return new(
                false,
                exception);
        }
    }

    private async Task Settle(
        OwnedEntry<InboxEntry> owned,
        Exception exception,
        TimeSpan settlementTimeout,
        CancellationToken claimLost)
    {
        using var timeout =
            new CancellationTokenSource(settlementTimeout);

        using var cancellation =
            CancellationTokenSource.CreateLinkedTokenSource(
                timeout.Token,
                claimLost);

        var now = time.GetUtcNow();

        var failure = failures.Create(
            exception,
            owned.Entry.Attempts,
            now);

        var result = retry.Evaluate(
            exception,
            owned.Entry.Attempts);

        var settled = result.ShouldRetry
            ? await Retry(
                owned,
                failure,
                now,
                result.Delay,
                exception,
                cancellation.Token)
            : await Fault(
                owned,
                failure,
                now,
                exception,
                cancellation.Token);

        if (!settled)
        {
            ClaimLost(owned.Entry);
        }
    }

    private async ValueTask<bool> Retry(
        OwnedEntry<InboxEntry> owned,
        MessageFailure failure,
        DateTimeOffset now,
        TimeSpan delay,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var entry = owned.Entry;

        RetryScheduled(
            logger,
            entry.MessageId,
            entry.ConsumerId,
            entry.Attempts,
            delay,
            exception);

        var settled = await store.Retry(
            entry.Key,
            owned.ClaimId,
            failure,
            now.UtcDateTime,
            now.Add(delay).UtcDateTime,
            cancellationToken);

        if (settled)
        {
            MessageMetrics.Retried(MetricName);
        }

        return settled;
    }

    private async ValueTask<bool> Fault(
        OwnedEntry<InboxEntry> owned,
        MessageFailure failure,
        DateTimeOffset now,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var entry = owned.Entry;

        EntryDeadLettered(
            logger,
            entry.MessageId,
            entry.ConsumerId,
            entry.Attempts,
            exception);

        var settled = await store.DeadLetter(
            entry.Key,
            owned.ClaimId,
            failure,
            now.UtcDateTime,
            cancellationToken);

        if (settled)
        {
            MessageMetrics.DeadLettered(MetricName);
        }

        return settled;
    }

    private void Complete(InboxEntry entry)
    {
        MessageMetrics.Completed(MetricName);

        EntryCompleted(
            logger,
            entry.MessageId,
            entry.ConsumerId,
            entry.Attempts);
    }

    private void ClaimLost(InboxEntry entry)
    {
        MessageMetrics.LeaseLost(MetricName);

        EntryClaimLost(
            logger,
            entry.MessageId,
            entry.ConsumerId);
    }

    private static async Task StopRenewal(
        CancellationTokenSource cancellation,
        Task task)
    {
        await cancellation.CancelAsync();
        await task;
    }

    private readonly record struct AttemptOutcome(
        bool Completed,
        Exception? Failure);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Inbox entry {MessageId}/{ConsumerId} failed on attempt {Attempt}; retry in {Delay}.")]
    private static partial void RetryScheduled(
        ILogger logger,
        Guid messageId,
        string consumerId,
        int attempt,
        TimeSpan delay,
        Exception exception);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Inbox entry {MessageId}/{ConsumerId} dead-lettered on attempt {Attempt}.")]
    private static partial void EntryDeadLettered(
        ILogger logger,
        Guid messageId,
        string consumerId,
        int attempt,
        Exception exception);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Inbox entry {MessageId}/{ConsumerId} completed on attempt {Attempt}.")]
    private static partial void EntryCompleted(
        ILogger logger,
        Guid messageId,
        string consumerId,
        int attempt);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Inbox entry {MessageId}/{ConsumerId} lost its claim.")]
    private static partial void EntryClaimLost(
        ILogger logger,
        Guid messageId,
        string consumerId);
}
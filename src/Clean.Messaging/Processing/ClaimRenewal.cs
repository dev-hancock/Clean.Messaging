using Clean.Messaging.Abstractions;
using Clean.Messaging.Persistence;
using Microsoft.Extensions.Logging;

namespace Clean.Messaging.Processing;

internal sealed partial class ClaimRenewal(
    TimeProvider time,
    ILogger<ClaimRenewal> logger)
{
    public async Task Run<TEntry>(
        IDurableStore<TEntry> store,
        EntryKey key,
        string claimId,
        MessageLeaseOptions lease,
        CancellationTokenSource claimLost,
        CancellationToken cancellationToken)
        where TEntry : class, IDurableEntry
    {
        using var timer = new PeriodicTimer(
            lease.RenewalInterval,
            time);

        try
        {
            while (await timer.WaitForNextTickAsync(
                       cancellationToken))
            {
                var now = time.GetUtcNow().UtcDateTime;

                if (await store.Renew(
                        key,
                        claimId,
                        now,
                        now.Add(lease.Duration),
                        cancellationToken))
                {
                    continue;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await claimLost.CancelAsync();
                return;
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            RenewalFailed(
                logger,
                key.MessageId,
                key.ConsumerId,
                exception);

            await claimLost.CancelAsync();
        }
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Claim renewal failed for {MessageId}/{ConsumerId}.")]
    private static partial void RenewalFailed(
        ILogger logger,
        Guid messageId,
        string consumerId,
        Exception exception);
}

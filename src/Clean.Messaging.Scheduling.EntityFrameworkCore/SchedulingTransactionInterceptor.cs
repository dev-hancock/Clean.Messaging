using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Clean.Messaging.Scheduling.EntityFrameworkCore;

internal sealed class SchedulingTransactionInterceptor(
    SchedulingTracker tracker)
    : DbTransactionInterceptor
{
    public override void TransactionCommitted(
        DbTransaction transaction,
        TransactionEndEventData eventData)
    {
        if (eventData.Context is { } db)
        {
            tracker.TransactionCommitted(db);
        }
    }

    public override Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        TransactionCommitted(
            transaction,
            eventData);

        return Task.CompletedTask;
    }

    public override void TransactionRolledBack(
        DbTransaction transaction,
        TransactionEndEventData eventData)
    {
        if (eventData.Context is { } db)
        {
            tracker.TransactionRolledBack(db);
        }
    }

    public override Task TransactionRolledBackAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        TransactionRolledBack(
            transaction,
            eventData);

        return Task.CompletedTask;
    }

    public override void TransactionFailed(
        DbTransaction transaction,
        TransactionErrorEventData eventData)
    {
        if (eventData.Context is { } db)
        {
            tracker.TransactionFailed(db);
        }
    }

    public override Task TransactionFailedAsync(
        DbTransaction transaction,
        TransactionErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        TransactionFailed(
            transaction,
            eventData);

        return Task.CompletedTask;
    }
}

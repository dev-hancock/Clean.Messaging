using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Clean.Messaging.Outbox.EntityFrameworkCore;

internal sealed class OutboxTransactionInterceptor(
    OutboxTracker tracker,
    IOutboxCommitVerifier verifier)
    : DbTransactionInterceptor
{
    public override InterceptionResult TransactionCommitting(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result)
    {
        if (eventData.Context is { } db)
        {
            tracker.TransactionCommitting(db);
        }

        return result;
    }

    public override ValueTask<InterceptionResult> TransactionCommittingAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } db)
        {
            tracker.TransactionCommitting(db);
        }

        return ValueTask.FromResult(result);
    }

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
            ResolveFailure(db);
        }
    }

    public override async Task TransactionFailedAsync(
        DbTransaction transaction,
        TransactionErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } db)
        {
            await ResolveFailureAsync(db);
        }
    }

    public override InterceptionResult CreatingSavepoint(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result)
    {
        EnsureSavepointIsInternal(
            eventData.Context);

        return result;
    }

    public override ValueTask<InterceptionResult> CreatingSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(
            CreatingSavepoint(
                transaction,
                eventData,
                result));
    }

    public override InterceptionResult RollingBackToSavepoint(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result)
    {
        EnsureSavepointIsInternal(
            eventData.Context);

        return result;
    }

    public override ValueTask<InterceptionResult> RollingBackToSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(
            RollingBackToSavepoint(
                transaction,
                eventData,
                result));
    }

    private void ResolveFailure(
        DbContext db)
    {
        if (!tracker.IsCommitting(db))
        {
            tracker.TransactionRolledBack(db);
            return;
        }

        var keys =
            tracker.GetPendingKeys(db);

        if (keys.Length == 0)
        {
            tracker.TransactionCommitUnknown(db);
            return;
        }

        try
        {
            if (verifier.IsCommitted(keys))
            {
                tracker.TransactionCommitted(db);
            }
            else
            {
                tracker.TransactionRolledBack(db);
            }
        }
        catch
        {
            tracker.TransactionCommitUnknown(db);
        }
    }

    private async Task ResolveFailureAsync(
        DbContext db)
    {
        if (!tracker.IsCommitting(db))
        {
            tracker.TransactionRolledBack(db);
            return;
        }

        var keys =
            tracker.GetPendingKeys(db);

        if (keys.Length == 0)
        {
            tracker.TransactionCommitUnknown(db);
            return;
        }

        try
        {
            var committed =
                await verifier.IsCommittedAsync(
                    keys,
                    CancellationToken.None);

            if (committed)
            {
                tracker.TransactionCommitted(db);
            }
            else
            {
                tracker.TransactionRolledBack(db);
            }
        }
        catch
        {
            tracker.TransactionCommitUnknown(db);
        }
    }

    private void EnsureSavepointIsInternal(
        DbContext? db)
    {
        if (db is not null &&
            !tracker.IsSaving(db))
        {
            throw new NotSupportedException(
                "Caller-managed savepoints are not supported by outbox capture.");
        }
    }
}
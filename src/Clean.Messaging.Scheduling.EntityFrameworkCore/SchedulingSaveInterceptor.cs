using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Clean.Messaging.Scheduling.EntityFrameworkCore;

internal sealed class SchedulingSaveInterceptor(
    SchedulingTracker tracker)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is { } db)
        {
            tracker.BeginSave(db);
        }

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(
            SavingChanges(
                eventData,
                result));
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        if (eventData.Context is { } db)
        {
            tracker.SaveSucceeded(db);
        }

        return result;
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } db)
        {
            tracker.SaveSucceeded(db);
        }

        return ValueTask.FromResult(result);
    }

    public override void SaveChangesFailed(
        DbContextErrorEventData eventData)
    {
        if (eventData.Context is { } db)
        {
            tracker.SaveFailed(db);
        }
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        SaveChangesFailed(eventData);
        return Task.CompletedTask;
    }

    public override void SaveChangesCanceled(
        DbContextEventData eventData)
    {
        if (eventData.Context is { } db)
        {
            tracker.SaveCanceled(db);
        }
    }

    public override Task SaveChangesCanceledAsync(
        DbContextEventData eventData,
        CancellationToken cancellationToken = default)
    {
        SaveChangesCanceled(eventData);
        return Task.CompletedTask;
    }
}

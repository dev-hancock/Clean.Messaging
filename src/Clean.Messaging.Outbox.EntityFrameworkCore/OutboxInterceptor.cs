using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Clean.Messaging.Outbox.EntityFrameworkCore;

public interface IOutboxCapture
{
    void Capture(DbContext db);
}

internal sealed class OutboxInterceptor(
    IEnumerable<IOutboxCapture> captures,
    OutboxTracker tracker)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Capture(eventData.Context);

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);

        return ValueTask.FromResult(result);
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
            tracker.SaveFailed(db);
        }
    }

    public override Task SaveChangesCanceledAsync(
        DbContextEventData eventData,
        CancellationToken cancellationToken = default)
    {
        SaveChangesCanceled(eventData);

        return Task.CompletedTask;
    }

    private void Capture(
        DbContext? db)
    {
        if (db is null)
        {
            return;
        }

        tracker.BeginSave(db);

        try
        {
            foreach (var capture in captures)
            {
                capture.Capture(db);
            }

            tracker.TrackEntries(db);
        }
        catch
        {
            tracker.SaveFailed(db);
            throw;
        }
    }
}

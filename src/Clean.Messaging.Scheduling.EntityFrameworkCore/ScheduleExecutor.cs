using Clean.Messaging.Scheduling.Delivery;
using Clean.Messaging.Scheduling.Persistence;
using Clean.Messaging.Scheduling.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Messaging.Scheduling.EntityFrameworkCore;

internal sealed class ScheduleExecutor<TDbContext>(
    TDbContext strategyContext,
    IServiceScopeFactory scopes,
    TimeProvider time)
    : IScheduleExecutor
    where TDbContext : DbContext
{
    public async ValueTask<bool> Dispatch(
        OwnedSchedule owned,
        CancellationToken cancellationToken)
    {
        var strategy = strategyContext.Database
            .CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            () => DispatchOwned(
                owned,
                cancellationToken));
    }

    private async Task<bool> DispatchOwned(
        OwnedSchedule owned,
        CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();

        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<TDbContext>();

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            if (!await TryBeginDispatch(
                    db,
                    owned,
                    cancellationToken))
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return await IsSettled(
                    db,
                    owned.Message.Id,
                    cancellationToken);
            }

            var scheduled = await Load(
                db,
                owned.Message.Id,
                cancellationToken);

            await Deliver(
                services,
                scheduled,
                cancellationToken);

            scheduled.Complete(
                time.GetUtcNow().UtcDateTime);

            await db.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return true;
        }
        catch
        {
            await Rollback(transaction);
            throw;
        }
    }

    private async Task<bool> TryBeginDispatch(
        TDbContext db,
        OwnedSchedule owned,
        CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow()
            .UtcDateTime;

        var affected = await db.Set<ScheduleEntry>()
            .Where(message =>
                message.Id == owned.Message.Id &&
                message.ClaimId == owned.ClaimId &&
                message.ClaimedUntil != null &&
                message.ClaimedUntil > now &&
                message.DispatchedAt == null &&
                message.CancelledAt == null &&
                message.FailedAt == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        message => message.Version,
                        message => message.Version + 1),
                cancellationToken);

        return affected == 1;
    }

    private static Task<ScheduleEntry> Load(
        TDbContext db,
        ScheduleId id,
        CancellationToken cancellationToken)
    {
        return db.Set<ScheduleEntry>()
            .SingleAsync(
                message => message.Id == id,
                cancellationToken);
    }

    private static async ValueTask Deliver(
        IServiceProvider services,
        ScheduleEntry schedule,
        CancellationToken cancellationToken)
    {
        var deliveries = services
            .GetRequiredService<ScheduleDeliveryRegistry>();

        var delivery = deliveries.Get(
            schedule.Target);

        await delivery.Dispatch(
            new(
                schedule.Id,
                schedule.GroupId,
                schedule.Message,
                schedule.CorrelationId,
                schedule.CausationId),
            cancellationToken);
    }

    private static async Task<bool> IsSettled(
        TDbContext db,
        ScheduleId id,
        CancellationToken cancellationToken)
    {
        var state = await db.Set<ScheduleEntry>()
            .AsNoTracking()
            .Where(message => message.Id == id)
            .Select(message => new
            {
                message.DispatchedAt,
                message.CancelledAt,
                message.FailedAt
            })
            .SingleOrDefaultAsync(
                cancellationToken);

        return state is null ||
               state.DispatchedAt is not null ||
               state.CancelledAt is not null ||
               state.FailedAt is not null;
    }

    private static async Task Rollback(
        IDbContextTransaction transaction)
    {
        try
        {
            await transaction.RollbackAsync(
                CancellationToken.None);
        }
        catch
        {
            // Preserve the original processing or uncertain-commit failure.
        }
    }
}
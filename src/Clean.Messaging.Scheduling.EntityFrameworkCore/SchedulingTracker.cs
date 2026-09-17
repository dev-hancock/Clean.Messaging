using Clean.Messaging.Scheduling.Persistence;
using Clean.Messaging.Scheduling.Runtime;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace Clean.Messaging.Scheduling.EntityFrameworkCore;

internal sealed class SchedulingTracker(
    ISchedulingEngine engine)
{
    private readonly ConditionalWeakTable<DbContext, State> _states = new();

    public void BeginSave(
        DbContext db)
    {
        var changed = db.ChangeTracker
            .Entries<ScheduleEntry>()
            .Any(entry => entry.State is
                EntityState.Added or
                EntityState.Modified or
                EntityState.Deleted);

        if (!changed)
        {
            return;
        }

        if (Transaction.Current is not null)
        {
            throw new InvalidOperationException(
                "Scheduled message persistence does not support ambient transactions. Use an EF transaction.");
        }

        var state = _states.GetOrCreateValue(db);

        if (state.Saving)
        {
            throw new InvalidOperationException(
                "Nested SaveChanges is not supported by scheduled message tracking.");
        }

        state.Saving = true;
        state.Changed = true;
    }

    public void SaveSucceeded(
        DbContext db)
    {
        if (!_states.TryGetValue(db, out var state) ||
            !state.Saving)
        {
            return;
        }

        state.Saving = false;
        state.Pending |= state.Changed;
        state.Changed = false;

        if (db.Database.CurrentTransaction is null)
        {
            WakeAndClear(db);
        }
    }

    public void SaveFailed(
        DbContext db)
    {
        if (!_states.TryGetValue(db, out var state) ||
            !state.Saving)
        {
            return;
        }

        state.Saving = false;

        if (state.Changed)
        {
            engine.Wake();
        }

        state.Changed = false;

        if (!state.Pending)
        {
            _states.Remove(db);
        }
    }

    public void SaveCanceled(
        DbContext db)
    {
        if (!_states.TryGetValue(db, out var state) ||
            !state.Saving)
        {
            return;
        }

        state.Saving = false;

        if (state.Changed)
        {
            engine.Wake();
        }

        state.Changed = false;

        if (!state.Pending)
        {
            _states.Remove(db);
        }
    }

    public void TransactionCommitted(
        DbContext db)
    {
        WakeAndClear(db);
    }

    public void TransactionRolledBack(
        DbContext db)
    {
        _states.Remove(db);
    }

    public void TransactionFailed(
        DbContext db)
    {
        WakeAndClear(db);
    }

    private void WakeAndClear(
        DbContext db)
    {
        if (!_states.TryGetValue(db, out var state))
        {
            return;
        }

        if (state.Pending ||
            state.Changed)
        {
            engine.Wake();
        }

        _states.Remove(db);
    }

    private sealed class State
    {
        public bool Saving { get; set; }

        public bool Changed { get; set; }

        public bool Pending { get; set; }
    }
}

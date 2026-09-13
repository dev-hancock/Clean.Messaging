using Clean.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace Clean.Messaging.Outbox.EntityFrameworkCore;

public sealed class OutboxTracker(
    OutboxSignal signal)
{
    private readonly ConditionalWeakTable<DbContext, ContextState> _states = new();

    public bool IsSaving(
        DbContext db)
    {
        return _states.TryGetValue(db, out var state) &&
               state.CurrentSave is not null;
    }

    public bool IsCommitting(
        DbContext db)
    {
        return _states.TryGetValue(db, out var state) &&
               state.CommitInProgress;
    }

    public void BeginSave(
        DbContext db)
    {
        if (Transaction.Current is not null)
        {
            throw new InvalidOperationException(
                "Outbox capture does not support ambient transactions. Use an EF transaction.");
        }

        var state = _states.GetOrCreateValue(db);
        var transactionId = db.Database.CurrentTransaction?.TransactionId;

        if (state.CurrentSave is not null)
        {
            throw new InvalidOperationException(
                "Nested SaveChanges is not supported.");
        }

        if (state.TransactionId != transactionId &&
            !state.PendingCommit.IsEmpty)
        {
            throw new InvalidOperationException(
                "The outbox transaction changed without an observed commit or rollback. " +
                "Discard this DbContext after an uncommitted transaction.");
        }

        state.TransactionId = transactionId;
        state.CurrentSave = new();

        foreach (var entry in AddedEntries(db))
        {
            state.CurrentSave.ExistingKeys.Add(
                entry.Entity.Key);
        }
    }

    public bool Capture(
        DbContext db,
        Guid id,
        Action acknowledge)
    {
        ArgumentNullException.ThrowIfNull(
            acknowledge);

        var state = CurrentSave(db);

        if (state.PendingCommit.Captures.Contains(id) ||
            !state.CurrentSave!.Captures.Add(id))
        {
            return false;
        }

        state.CurrentSave.Acknowledgements.Add(
            id,
            acknowledge);

        return true;
    }

    public void TrackEntries(
        DbContext db)
    {
        var current =
            CurrentSave(db).CurrentSave!;

        foreach (var entry in AddedEntries(db))
        {
            var key = entry.Entity.Key;

            current.Keys.Add(key);

            if (!current.ExistingKeys.Contains(key))
            {
                current.CapturedKeys.Add(key);
            }
        }
    }

    public void SaveSucceeded(
        DbContext db)
    {
        var state =
            _states.GetOrCreateValue(db);

        if (state.CurrentSave is not null)
        {
            state.PendingCommit.Merge(
                state.CurrentSave);

            state.CurrentSave = null;
        }

        if (db.Database.CurrentTransaction is null)
        {
            Commit(db);
        }
    }

    public void SaveFailed(
        DbContext db)
    {
        if (!_states.TryGetValue(db, out var state) ||
            state.CurrentSave is null)
        {
            return;
        }

        TrackEntries(db);

        Detach(
            db,
            state.CurrentSave.CapturedKeys);

        state.CurrentSave = null;
    }

    public void TransactionCommitting(
        DbContext db)
    {
        if (!_states.TryGetValue(db, out var state) ||
            state.PendingCommit.IsEmpty)
        {
            return;
        }

        state.CommitInProgress = true;
    }

    public void TransactionCommitted(
        DbContext db)
    {
        if (_states.TryGetValue(db, out var state))
        {
            state.CommitInProgress = false;
        }

        if (!IsSaving(db))
        {
            Commit(db);
        }
    }

    public EntryKey[] GetPendingKeys(
        DbContext db)
    {
        if (!_states.TryGetValue(db, out var state) ||
            !state.CommitInProgress)
        {
            return [];
        }

        return state.PendingCommit.Keys
            .ToArray();
    }

    public void TransactionCommitUnknown(
        DbContext db)
    {
        if (_states.TryGetValue(db, out var state))
        {
            state.CommitInProgress = false;
        }
    }

    public void TransactionRolledBack(
        DbContext db)
    {
        if (!_states.TryGetValue(db, out var state))
        {
            return;
        }

        if (state.TransactionId is null)
        {
            return;
        }

        if (state.CurrentSave is not null)
        {
            Detach(
                db,
                state.CurrentSave.Keys);
        }

        Detach(
            db,
            state.PendingCommit.Keys);

        _states.Remove(db);
    }

    private ContextState CurrentSave(
        DbContext db)
    {
        var state =
            _states.GetOrCreateValue(db);

        if (state.CurrentSave is null)
        {
            throw new InvalidOperationException(
                "Outbox capture requires an active SaveChanges operation.");
        }

        return state;
    }

    private void Commit(
        DbContext db)
    {
        if (!_states.TryGetValue(db, out var state))
        {
            return;
        }

        foreach (var acknowledge in
                 state.PendingCommit.Acknowledgements.Values)
        {
            acknowledge();
        }

        if (state.PendingCommit.Keys.Count > 0)
        {
            signal.Wake();
        }

        _states.Remove(db);
    }

    private static IEnumerable<EntityEntry<OutboxEntry>> AddedEntries(
        DbContext db)
    {
        return db.ChangeTracker
            .Entries<OutboxEntry>()
            .Where(entry =>
                entry.State == EntityState.Added);
    }

    private static void Detach(
        DbContext db,
        IReadOnlySet<EntryKey> keys)
    {
        var entries = db.ChangeTracker
            .Entries<OutboxEntry>()
            .Where(entry =>
                keys.Contains(entry.Entity.Key))
            .ToArray();

        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }

    private sealed class ContextState
    {
        public Guid? TransactionId { get; set; }

        public SaveState? CurrentSave { get; set; }

        public SaveState PendingCommit { get; } = new();

        public bool CommitInProgress { get; set; }
    }

    private sealed class SaveState
    {
        public HashSet<Guid> Captures { get; } = [];

        public Dictionary<Guid, Action> Acknowledgements { get; } = [];

        public HashSet<EntryKey> Keys { get; } = [];

        public HashSet<EntryKey> ExistingKeys { get; } = [];

        public HashSet<EntryKey> CapturedKeys { get; } = [];

        public bool IsEmpty =>
            Keys.Count == 0 &&
            Captures.Count == 0;

        public void Merge(
            SaveState other)
        {
            Captures.UnionWith(
                other.Captures);

            Keys.UnionWith(
                other.Keys);

            foreach (var (id, acknowledge) in
                     other.Acknowledgements)
            {
                Acknowledgements.TryAdd(
                    id,
                    acknowledge);
            }
        }
    }
}
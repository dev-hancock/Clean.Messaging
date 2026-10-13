# OUTBOX-007 — Save failure, cancellation, rollback and retry tracking

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: OutboxTracker.BeginSave/TrackEntries/SaveFailed/TransactionRolledBack and interceptor failure/cancellation hooks. No claim that all business tracking is restored.

Preconditions: tracked active save or observed explicit transaction. Postconditions: a failed/cancelled save removes newly captured rows for that save, not pre-staged rows or all prior successful pending captures; no acknowledgement occurs. Observed rollback removes recorded transaction work and tracker state; subsequent retry needs application state/captures reconciled by caller. TransactionRolledBack without recorded transaction ID returns without inventing rollback evidence.

## OUTBOX-007-AC-01 — Detach current captured rows on failed save

- **Given** pre-staged rows, earlier successful transaction captures and newly captured rows coexist.
- **When** the current save fails or is cancelled.
- **Then** newly captured current-save entries detach; pre-staged/earlier pending entries are not all discarded and acknowledgement does not run.

## OUTBOX-007-AC-02 — Retain events for retry and clear observed rollback keys

- **Given** captured events remain in memory while a save fails, or an explicit transaction rollback is observed.
- **When** a later valid save/capture is attempted with reconciled application state.
- **Then** failed events can be captured again; observed rollback removes recorded outgoing tracked keys without pretending to revert arbitrary domain state.

## OUTBOX-007-AC-03 — Reject changed transaction identity with pending work

- **Given** pending captured/staged keys belong to an earlier transaction whose end was not observed.
- **When** BeginSave occurs after that transaction identity changes.
- **Then** it fails rather than silently acknowledging or transferring pending work into another transaction.

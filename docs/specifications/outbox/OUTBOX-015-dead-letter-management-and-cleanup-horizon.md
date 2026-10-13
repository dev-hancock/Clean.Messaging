# OUTBOX-015 — Dead-letter management and cleanup horizon

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: IOutboxManager/OutboxManager, OutboxCleanupWorker, shared DeadLetterStore and DurableStore.Cleanup. Outbox has no replay-from-admissions API.

Contracts: GetDeadLetters(default 100), Requeue, Discard; inherited Cleanup. Positive listing/cleanup limit and nonnull key collections; empty allowed. Per-key independent transactions, no batch atomicity; ambient mutations rejected. Requeue resets Attempts and claim/due/terminal state but retains diagnostics; discard preserves dead-letter diagnosis. Cleanup immediate/periodic one batch, only terminal timestamps strictly older than UTC retention cutoff. Deletes identity/completion history; pending work survives, no admission archive or outgoing replay is provided.

## OUTBOX-015-AC-01 — Inspect and manage eligible dead letters

- **Given** eligible, discarded, absent/completed and duplicate keys, or invalid listing/empty mutation inputs.
- **When** manager operations run.
- **Then** positive listing returns nondiscarded dead letters; nonpositive fails; only eligible transitions count, empty collections do nothing, duplicates count Requested but not extra transitions.

## OUTBOX-015-AC-02 — Preserve diagnosis and partial transitions

- **Given** several eligible keys and later failure/cancellation or ambient Transaction.Current.
- **When** Requeue/Discard executes.
- **Then** successful earlier independent transitions persist; requeue resets Attempts/terminal/claim/due while preserving diagnosis; discard retains dead-letter state; ambient mutation is rejected.

## OUTBOX-015-AC-03 — Delete only old terminal evidence

- **Given** pending and terminal rows lie before/equal/after cutoff.
- **When** Cleanup runs with positive bounded limit.
- **Then** only strictly older terminal rows are deleted up to limit; pending/equality rows survive and deleted logical pairs can later be staged again.

## OUTBOX-015-AC-04 — Do not hide cleanup failure

- **Given** a correctly resolved cleanup worker encounters ordinary storage failure or host cancellation.
- **When** its cleanup cycle runs.
- **Then** ordinary failure is warning-logged for later retry; host cancellation stops and neither path claims successful deletion.

## OUTBOX-015-AC-05 — Reject null key collections and invalid cleanup limit

- **Given** Requeue/Discard receives null or Cleanup receives a nonpositive limit.
- **When** the operation is requested.
- **Then** invalid input fails rather than performing an empty successful operation.

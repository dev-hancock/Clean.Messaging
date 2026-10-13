# OUTBOX-006 — Public save capture protocol and per-context deduplication

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: ISaveCapture.Capture, OutboxInterceptor save success/failure/cancellation callbacks and public OutboxTracker operations. [Persistence](../../reference/persistence.md) is descriptive.

Contracts: OutboxTracker IsSaving/IsCommitting, BeginSave, Capture<TCapture>, AcknowledgeAfterCommit, TrackEntries, SaveSucceeded/SaveFailed, TransactionCommitting/Committed/CommitUnknown/RolledBack, GetPendingKeys. State belongs to each DbContext; capture uniqueness is (capture type,eventId), acknowledgement uniqueness is eventId only. BeginSave validates ambient/nesting/pending transaction identity. Capture/ack/track require active save; acknowledgement callback nonnull. Successful save merges capture/key/ack state; explicit transaction defers callbacks. Newly captured current-save rows are distinct from pre-staged and previous pending-save rows.

## OUTBOX-006-AC-01 — Deduplicate captures across saves in one transaction

- **Given** the same capture type/event ID is encountered repeatedly in one context/explicit transaction.
- **When** successful saves repeat before commit.
- **Then** Capture authorizes the first only; pending acknowledgements stay deferred until observed commit, with no duplicate capture rows.

## OUTBOX-006-AC-02 — Distinguish contexts and capture types

- **Given** two contexts or two different capture types encounter the same event ID.
- **When** Capture operates inside each active save.
- **Then** capture state is independent per context/type; acknowledgement callbacks keyed only by eventId do not imply one callback per capture type.

## OUTBOX-006-AC-03 — Reject invalid capture protocol

- **Given** Capture/ack/TrackEntries occurs without BeginSave, acknowledgement is null, save is nested, or ambient transaction is present.
- **When** the public tracker operation is requested.
- **Then** invalid lifecycle/input is rejected rather than silently treated as a successful capture.

## OUTBOX-006-AC-04 — Expose saving/committing and pending-key boundaries

- **Given** active saving, ordinary idle state and a marked commit phase.
- **When** IsSaving, IsCommitting and GetPendingKeys are inspected.
- **Then** saving/committing state reflects the phase; pending keys are exposed during commit rather than proving transaction success.

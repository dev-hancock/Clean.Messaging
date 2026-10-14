# OPS-001 — Dead-letter listing and transitions

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Operators need to identify retained failed targets and deliberately retry or discard them without resetting successful work. Management must preserve the failed operation’s identity and diagnostics while making its changed lifecycle visible.

## Guarantees

- Positive limited dead-letter listing MUST return only retained, non-discarded dead letters with their public diagnostic snapshot, ordered by dead-letter time without a tie-order promise.
- Successful requeue MUST restore unresolved immediate eligibility, reset Attempts to zero and clear ownership/dead-letter status while preserving identity, payload and failure history.
- Successful discard MUST remove a target from dead-letter listing while retaining its discarded outcome until cleanup.
- Absent, discarded or successful targets MUST NOT be reset by normal requeue/discard; an ineligible mutation MUST return false.
- Standard requeue/discard MUST reject ambient transactions with InvalidOperationException rather than silently change their independent transaction boundary.

## Contract boundaries

These guarantees concern a retained, normally managed target and the standard store. Listing is a snapshot, not a durable audit stream. Requeue restores a retry opportunity, not a new event or guaranteed eventual success. Manager collections do not establish an all-key atomic batch. Provider-specific ambiguous mutation attribution remains a separate question.

## Acceptance criteria

### OPS-001-AC-01 — List projection

- **Given** dead-lettered, discarded and successful targets coexist.
- **When** a positive limited dead-letter listing is requested.
- **Then** only non-discarded dead letters appear with Key, EventId, dead-letter time, Attempts, Code and Error, ordered by dead-letter time without a tie-order guarantee.

### OPS-001-AC-02 — Requeue reset

- **Given** a retained non-discarded dead letter has previous failure diagnostics.
- **When** Requeue succeeds.
- **Then** it is unresolved and immediately eligible with Attempts zero and cleared ownership/dead-letter status, retaining its identity, payload and failure history.

### OPS-001-AC-03 — Discard visibility

- **Given** a retained non-discarded dead letter exists.
- **When** Discard succeeds.
- **Then** it is absent from dead-letter listing but remains stored with a discarded outcome until cleanup.

### OPS-001-AC-04 — Ineligible mutation

- **Given** a key is absent, already discarded or successful.
- **When** normal Requeue or Discard is requested.
- **Then** the operation returns false and does not reset successful work.

### OPS-001-AC-05 — Independent mutation restriction

- **Given** an ambient TransactionScope is active.
- **When** standard Requeue or Discard is requested.
- **Then** InvalidOperationException rejects that unsupported transaction boundary rather than silently suppressing it.

## Open decisions

- [GAP-CORE-005](../GAPS.md#gap-core-005): the related policy remains undecided; its recommendation is not approved.

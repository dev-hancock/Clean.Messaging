# OUTBOX-009 — Savepoints, ambient transactions and observer restrictions

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Host authors need explicit limits on transaction observation so savepoints or ambient/unobserved transaction ends cannot silently invalidate the capture acknowledgement boundary.

## Guarantees

- Caller-managed savepoints outside an active observed save MUST be rejected when using this capture integration rather than imply supported capture rollback semantics.
- Provider save-time savepoints within an active observed EF save MUST remain allowed as part of supported save integration.
- An unobserved end of a transaction with pending capture MUST NOT be accepted as proof of commit or acknowledgement; subsequent inconsistent lifecycle participation MUST fail.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Public save/transaction integration compatibility and coding standards sections 7, 8 and 14 establish explicit unsupported boundaries and commit-before-acknowledgement; no arbitrary transaction protocol is promised.

## Contract boundaries

The supported protocol observes save and transaction completion through configured integration. TransactionScope and caller-managed savepoint recovery are not substitutes for it. These restrictions do not claim rollback of arbitrary tracked domain state and do not choose a future savepoint protocol.

## Acceptance criteria

### OUTBOX-009-AC-01 — Reject user savepoints outside active save

- **Given** pending capture integration is used and no active observed save permits a caller savepoint.
- **When** the caller creates or rolls back to a savepoint.
- **Then** NotSupportedException reports the unsupported savepoint boundary instead of silently permitting a partial capture-state rewind.

### OUTBOX-009-AC-02 — Allow EF save-time savepoints

- **Given** an observed EF save is active and its provider uses internal save-time savepoints.
- **When** the supported save executes.
- **Then** those internal savepoints are allowed rather than mistaken for unsupported caller-managed recovery, and save success/failure still determines capture bookkeeping.

### OUTBOX-009-AC-03 — Expose unobserved transaction end restriction

- **Given** an owning transaction ends without the integration observing completion and pending work remains.
- **When** capture participation later attempts to continue under an inconsistent transaction lifecycle.
- **Then** the unsupported state fails and does not falsely acknowledge the unobserved transaction as committed.

## Open decisions

None.

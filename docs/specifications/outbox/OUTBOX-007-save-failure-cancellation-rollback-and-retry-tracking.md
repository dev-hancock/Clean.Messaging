# OUTBOX-007 — Save failure, cancellation, rollback and retry tracking

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

A failed save or known rollback must leave capture integrations able to retry their still-unacknowledged facts, without detaching caller-owned staged state or accidentally transferring pending work to another transaction.

## Guarantees

- Failed or cancelled save MUST NOT acknowledge capture as committed. Only newly captured rows owned by that failed save MUST be detached; earlier pending successful-save capture and caller-prestaged rows MUST remain owned by their existing lifecycle.
- Known transaction rollback MUST clear recorded pending capture/key ownership and detach its tracked captured entries so the original facts can be retried.
- Pending work MUST NOT silently migrate to a changed owning transaction identity; that unsupported lifecycle MUST fail visibly.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Public tracker lifecycle compatibility and coding standards sections 6, 7, 8 and 9 establish scoped state ownership, failure preservation, no premature acknowledgement and retry-safe local capture.

## Contract boundaries

Recorded capture entries and arbitrary application tracking state are different ownership sets. Known rollback does not undo domain object mutations or external calls. Commit uncertainty must not be relabeled known rollback; its evidence policy remains in [OUTBOX-008](OUTBOX-008-commit-verification-and-uncertain-outcome.md).

## Acceptance criteria

### OUTBOX-007-AC-01 — Detach current captured rows on failed save

- **Given** a save includes newly captured rows, caller-prestaged rows and earlier successful pending-save captures.
- **When** that save fails or is cancelled.
- **Then** only newly captured current-save rows detach, earlier pending and prestaged rows remain, and no fact is acknowledged as committed.

### OUTBOX-007-AC-02 — Retain events for retry and clear observed rollback keys

- **Given** captured events remain unacknowledged after save failure or an observed owning-transaction rollback.
- **When** capture is retried in its valid lifecycle.
- **Then** facts remain available to recapture, and known rollback clears recorded pending keys/capture state and detaches its recorded entries rather than preserving stale ownership.

### OUTBOX-007-AC-03 — Reject changed transaction identity with pending work

- **Given** pending capture belongs to one transaction but another transaction identity is presented.
- **When** a new save begins with that pending work.
- **Then** the changed owning identity is rejected rather than silently merging the transactions.

## Open decisions

None.

# OUTBOX-005 — Atomic owning save and commit boundary

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

An application must not commit the state that justifies an outgoing operation without durably recording that operation, or publish an operation whose owning local transaction did not commit.

## Guarantees

- Participating application state and its staged outgoing target work MUST commit as one atomic local result.
- Uncommitted staged work MUST NOT be authorized for durable delivery or presented as committed by a wake hint or capture acknowledgement.
- A known rollback MUST leave no committed participating application change or newly captured outgoing work from that transaction.
- An ambiguous commit MUST remain uncertain to the caller rather than be inferred rolled back merely from a failed response.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 8 and 9 explicitly require owning-state/outgoing-work atomic capture and prohibit extending it to external publication.

## Contract boundaries

Application changes and outgoing entries must use the configured scoped relational context and required save/transaction integration, with installed schema. Another context/database, raw independent write, external call and broker publication are outside this atomic result. Rollback concerns database state, not arbitrary in-memory domain reset. Save success inside an explicit transaction is not commit success.

## Acceptance criteria

### OUTBOX-005-AC-01 — Commit application state with outgoing work

- **Given** application changes and outgoing targets participate in the configured owning context/transaction.
- **When** the owning transaction successfully commits.
- **Then** both the application state and outgoing targets are durable together and available for postcommit processing without a partial participating result.

### OUTBOX-005-AC-02 — Do not publish uncommitted work

- **Given** outgoing work is staged or saved within an explicit transaction but not committed.
- **When** processing or postcommit notification is considered.
- **Then** uncommitted work is not authorized for durable publication, and commit-based acknowledgement and wake stay deferred; no other wake hint is evidence that it committed.

### OUTBOX-005-AC-03 — Roll back the owning transaction

- **Given** the owning transaction rolls back with a known outcome.
- **When** the application observes its persistence result.
- **Then** neither its participating changes nor its newly captured outgoing targets committed, while arbitrary in-memory objects and external effects are not promised restored.

## Open decisions

None.

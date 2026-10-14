# DOMAIN-009 — Crash, recovery and retention boundaries

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications need to know which facts survive a process loss and which delivery evidence remains available after retention. Committed outbox targets, not the old aggregate object or a wake hint, carry the recovery responsibility.

## Guarantees

- Retained committed outgoing targets MUST remain recoverable independently of the source object’s in-memory acknowledgement and process-local wake.
- Raising alone MUST NOT be represented as durable recording or automatic reconstruction of pending facts after a crash.
- Outbox cleanup MUST NOT be represented as a permanent domain-event archive or permanent duplicate-suppression evidence.

## Contract boundaries

Recovery requires retained outgoing rows, resolvable contracts/targets and an operational host/store. It follows outbox leases and retry policy and can repeat downstream effects. This package is not event sourcing: loading an entity does not rehydrate its pending list from outbox history. No retention period or historical domain replay API is selected here.

## Acceptance criteria

### DOMAIN-009-AC-01 — Do not recover uncaptured in-memory facts automatically

- **Given** a fact was raised but never captured and committed.
- **When** the process crashes and a fresh aggregate/context loads.
- **Then** this capability supplies no durable pending-fact recovery or event-store reconstruction for that fact.

### DOMAIN-009-AC-02 — Recover committed delivery without source acknowledgement

- **Given** the application/outbox transaction committed but the process stopped before acknowledgement or wake.
- **When** a later operational host recovers the retained outgoing rows.
- **Then** their targets can be delivered without the old source object, with the normal possibility of repeated downstream effects.

### DOMAIN-009-AC-03 — Bound durable evidence by outbox retention

- **Given** completed event targets have been removed by outbox cleanup.
- **When** a caller requests historical domain replay or resubmits the logical ID.
- **Then** this capability supplies no domain history/replay and the removed outgoing evidence cannot provide permanent duplicate suppression.

## Open decisions

- [GAP-DUR-008](../GAPS.md#gap-dur-008): the related policy remains undecided; its recommendation is not approved.

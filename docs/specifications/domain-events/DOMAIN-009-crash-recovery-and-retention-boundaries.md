# DOMAIN-009 — Crash, recovery and retention boundaries

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: AggregateRoot in-memory list, capture/ack bookkeeping, outbox persisted entries and shared recovery/cleanup. [Durable workflows](../../explanation/durable-workflows.md) and [delivery semantics](../../explanation/delivery-semantics.md) explain boundaries.

Invariants: in-memory raising is not durable recording; captured delivery is driven by outbox after commit, independent of source event removal. Crash before capture can lose pending events; crash after durable commit can lose in-memory acknowledgement but not retained outgoing rows. Reloading an entity does not automatically reconstruct its pending event list from outbox. Outbox cleanup removes target/dedup evidence after retention; no domain event history or domain replay API is supplied.

## DOMAIN-009-AC-01 — Do not recover uncaptured in-memory facts automatically

- **Given** an event was raised but no owning capture/commit persisted it.
- **When** the process crashes and a fresh aggregate/context loads.
- **Then** this capability provides no durable pending-event recovery or event-store reconstruction for that fact.

## DOMAIN-009-AC-02 — Recover committed delivery without source acknowledgement

- **Given** domain/outbox transaction committed and process crashed before in-memory acknowledgement or wake.
- **When** host restarts with retained outgoing rows.
- **Then** outbox recovery can deliver the persisted targets independently of the old source object; downstream effects can repeat according to delivery boundaries.

## DOMAIN-009-AC-03 — Bound durable evidence by outbox retention

- **Given** captured event targets have completed and their rows are later cleaned.
- **When** a caller asks for historical domain replay or submits the same logical ID again.
- **Then** no domain history/replay is supplied; expired outgoing dedup evidence cannot promise permanent suppression.

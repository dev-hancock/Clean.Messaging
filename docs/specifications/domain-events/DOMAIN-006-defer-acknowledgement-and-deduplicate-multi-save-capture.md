# DOMAIN-006 — Defer acknowledgement and deduplicate multi-save capture

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: DomainEventCapture registers callback before factory/writer; OutboxTracker Capture/AcknowledgeAfterCommit/SaveSucceeded/TransactionCommitted. [Outbox specification](../outbox/OUTBOX-006-public-save-capture-protocol-and-per-context-deduplication.md#outbox-006--public-save-capture-protocol-and-per-context-deduplication) covers the shared tracker protocol.

Preconditions: stable pending source and explicit owning transaction observed by interceptors. Postconditions: same event/type captures once across successful saves before commit; acknowledgements run only after successful observed/verified commit. Events raised between saves can join pending transaction work. Acknowledge callback carries source/event identity but callback dictionary keyed solely eventId. No permanent inbox-like event admission ledger exists for these in-memory facts.

## DOMAIN-006-AC-01 — Keep facts pending across multiple saves

- **Given** a source event was captured in the first save of an explicit transaction.
- **When** further saves occur before commit then transaction commits.
- **Then** the event remains pending before commit, duplicate capture is suppressed, then normal acknowledgement removes it after commit.

## DOMAIN-006-AC-02 — Capture newly raised facts in later save

- **Given** an initial event is pending in transaction state and a distinct event is raised before a second save.
- **When** second save and final commit succeed.
- **Then** both facts have their respective outgoing target sets committed and acknowledged without recapturing the earlier event.

## DOMAIN-006-AC-03 — Acknowledge only selected committed identities

- **Given** an event is raised after the last capture but before acknowledgement processing without corrupting enumeration.
- **When** earlier captured work commits.
- **Then** acknowledgement removes the captured ID only; the newer uncaptured event remains pending.

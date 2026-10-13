# OUTBOX-010 — Postcommit callbacks and wake hints

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: OutboxTracker.Commit and PendingCommit.IsEmpty. [GAP-DUR-009](../GAPS.md#gap-dur-009) records callback failure/zero-key ambiguities.

Postcondition on normal observed commit: synchronous acknowledgements execute before wake, then state removal. Callback exceptions are not isolated and cannot roll back a durable commit. Pending IsEmpty checks keys/captures, not callbacks alone; callback-only custom integration can fail to participate in commit tracking. No durable callback ledger or retry policy exists. Wake is a coalesced hint; persisted rows remain recoverable if it is lost.

## OUTBOX-010-AC-01 — Acknowledge capture rather than delivery

- **Given** a successful observed commit includes captured events and outgoing keys.
- **When** commit callbacks finish normally.
- **Then** events are acknowledged and outgoing work is signalled without proving any target consumer or external transport has completed.

## OUTBOX-010-AC-02 — Surface callback failure after durable commit

- **Given** a custom acknowledgement callback throws before later callbacks/wake/state removal.
- **When** commit bookkeeping runs.
- **Then** the database can already be committed while acknowledgement sequence/wake/cleanup is interrupted; recovery can still discover persisted rows.

## OUTBOX-010-AC-03 — Observe callback-only participation gap

- **Given** a custom integration registers acknowledgement only with no captures or keys.
- **When** save/transaction bookkeeping examines pending state.
- **Then** callbacks alone do not make PendingCommit nonempty; registration is not a proven durable postcommit scheduling facility.

## OUTBOX-010-AC-04 — Distinguish ordinary callback failure from commit-failure reconciliation

- **Given** a callback throws while verified committed handling runs inside the transaction-failure reconciliation catch.
- **When** the interceptor handles that exception.
- **Then** the reconciliation catch can classify tracker state as unknown without rethrowing that callback exception; durable commit remains possible and later callbacks/wake/cleanup can be incomplete.

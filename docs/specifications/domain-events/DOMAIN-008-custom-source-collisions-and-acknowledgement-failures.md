# DOMAIN-008 — Custom source collisions and acknowledgement failures

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: capture-type/event-ID dedup across tracked sources, event-ID-only callback dictionary and synchronous postcommit callbacks. [GAP-DUR-010](../GAPS.md#gap-dur-010), [GAP-DUR-009](../GAPS.md#gap-dur-009). Interface XML provides no uniqueness/nonthrowing/concurrency contract.

Purpose: expose risks at the source extension boundary. Preconditions for current normal behaviour: distinct stable IDs across sources and nonthrowing acknowledgement with stable enumeration. Current integration does not enforce these conditions. Postcommit callback failure cannot undo committed domain/outbox state and may interrupt later acknowledgements, wake and tracker cleanup. Duplicate IDs across sources suppress later source capture/ack; source order is not approved conflict resolution.

## DOMAIN-008-AC-01 — Observe duplicate IDs across tracked sources

- **Given** two sources supply the same event ID, possibly different payloads.
- **When** capture scans both inside one transaction.
- **Then** only the first authorized capture contributes work/ack; the second can remain pending, without a payload-conflict error or global durable dedup guarantee.

## DOMAIN-008-AC-02 — Expose throwing acknowledgement after commit

- **Given** custom source Acknowledge throws and other sources also have callbacks.
- **When** owning transaction has committed and callbacks run.
- **Then** durable state can coexist with pending source events and interrupted later callbacks/wake; error does not imply transaction rollback.

## DOMAIN-008-AC-03 — Do not infer retry-safe custom acknowledgement

- **Given** a custom source removes more facts, does nothing or mutates collection during acknowledgement.
- **When** normal callback invokes its Acknowledge.
- **Then** integration does not independently verify event removal or enforce AggregateRoot semantics; observable results depend on the custom source.

## DOMAIN-008-AC-04 — Preserve observed ambiguity of callback error handling

- **Given** custom acknowledgement throws during commit-failure reconciliation after outgoing keys were verified.
- **When** the enclosing transaction-failure catch handles it.
- **Then** the callback error can be swallowed into unknown tracker state rather than universally surfaced; committed rows and unacknowledged source facts may coexist.

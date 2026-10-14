# INBOX-005 — Atomic local delivery and repeatable execution

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

An application's local state must not commit without the completion evidence that prevents the same Inbox target from applying those effects again. Conversely, a target must not appear complete while its local effects or outgoing work were lost. Recovery must preserve this consistency even when the worker does not know whether a commit succeeded.

## Guarantees

- Successful local delivery MUST commit participating application changes, completion of the individual consumer target and newly staged Outbox work as one atomic local result.
- A failed handler or loss of settlement authority before commit MUST NOT authorize committing that attempt's participating changes. Failure, cancellation and ownership loss MUST NOT be presented as successful completion.
- Retained committed completion MUST prevent recovery from repeating that target's committed local application effects. Completion of one target does not complete other consumers of the same message.
- Re-execution MUST preserve the original message identity and consumer-target boundary. Failure while rolling back MUST NOT replace the original failure.

These guarantees implement the [coding standards](../../CODING_STANDARDS.md) for local transaction consistency, per-target completion, stable identity and failure preservation. They concern durable local outcomes, not the number of handler invocations.

Coding standards sections 7, 8 and 9 explicitly establish local atomic consistency, failure preservation, stable identity and per-target completion; this is the established local transaction contract, not inferred choreography.

## Contract boundaries

The target is durably admitted, its consumer and message contract are resolvable, and local delivery is composed with the configured relational persistence context. Participating application changes and staged Outbox work must use that same scoped context and transaction boundary.

Changes made through another context, another database or an external service are outside this atomic result. Retained completion is necessary for duplicate protection; deleting that evidence ends this guarantee's recovery horizon. This specification neither chooses retention policy nor authorizes new replay targets.

Cancellation is cooperative. An interruption or ambiguous commit can leave the caller uncertain while the database has committed the complete local result. Failure reporting MUST NOT assert that rollback succeeded merely because commit did not return success. Atomicity forbids a partial participating commit, but does not promise that a handler ran once, that external effects rolled back, or that a caller immediately knows the commit outcome.

## Acceptance criteria

### INBOX-005-AC-01 — Commit scoped effects with completion

- **Given** an owned Inbox consumer target whose handler changes participating application state and stages outgoing work in the configured local transaction.
- **When** local delivery commits successfully.
- **Then** the application changes, completion of that consumer target and the staged outgoing work are durable together.

### INBOX-005-AC-02 — Roll back failure and ownership loss

- **Given** a handler has made participating changes, but fails or loses authority to complete its target before commit.
- **When** the local attempt ends without an ambiguous commit outcome.
- **Then** that attempt does not commit its application changes, staged outgoing work or successful target completion; the original failure remains a failure, and ownership loss is not reported as success.

### INBOX-005-AC-03 — Recover a committed target without repeating local effects

- **Given** the complete local result committed, its target-completion evidence remains retained, and the worker stopped before observing success.
- **When** that same target is dispatched again during recovery.
- **Then** delivery reports it completed without reapplying its committed local application effects.

### INBOX-005-AC-04 — Preserve uncertainty without claiming exactly once

- **Given** a commit outcome is ambiguous or local delivery is re-executed after an interruption.
- **When** retry or recovery runs.
- **Then** the same message and consumer-target identities are preserved, retained completion remains the evidence for avoiding repeated committed local effects, and any rollback failure does not replace the original failure; external effects are not represented as atomically committed or undone.

## Open decisions

This specification does not select an evidence-retention period or a new reconciliation protocol. Related unresolved choices remain in [GAP-DUR-004](../GAPS.md#gap-dur-004) and [GAP-DUR-008](../GAPS.md#gap-dur-008). Neither uncertainty nor a policy decision about retention permits a partial local result.

# TRANSPORT-007 — External publication and per-target completion are separate

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Applications need target progress tied to transport outcomes without pretending external publication joins their database transaction. Partial fan-out and abandoned ownership remain independently recoverable.

Authority: Coding standards and repository instructions explicitly require separate external/local boundaries, identity preservation, fencing, per-target progress and no exactly-once external-effect claim. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Failed external publication MUST NOT complete its durable target. Failure/retry settlement MUST remain separate from publication.
- Recovery of unresolved publication MUST preserve stored identities and permit repetition without representing external effects as rolled back.
- Only current settlement authority MUST complete a target; a stale owner MUST NOT overwrite another attempt.
- Failure/retry of one target MUST NOT reset or reinvoke an independently completed retained sibling.

## Contract boundaries

The configured transport defines publication success; success does not prove recipient application effects. Publication and completion are separate; accepted publication can repeat after failure/cancellation/uncertain settlement. Retention and retry/claim policy bound recovery. Fencing cannot revoke an issued external effect.

## Acceptance criteria

### TRANSPORT-007-AC-01 — Failed publication does not record successful completion

- **Given** an owned target.
- **When** publication fails before reporting success.
- **Then** the attempt does not complete that target; durable failure handling controls retry/fault classification.

### TRANSPORT-007-AC-02 — Publication success followed by lost settlement can repeat

- **Given** publication succeeded without durably established local completion.
- **When** the unresolved retained target is recovered and dispatched.
- **Then** publication can repeat with stable IDs, without exactly-once or rollback claims.

### TRANSPORT-007-AC-03 — Stale ownership cannot complete another attempt

- **Given** an old owner lost authority and a new owner acquired work.
- **When** the old external attempt finishes and tries to complete.
- **Then** the stale transition is rejected without completing the new owner's state, even if publication occurred.

### TRANSPORT-007-AC-04 — Sibling completion is independent

- **Given** a retained completed target and unresolved sibling.
- **When** the unresolved sibling retries or faults.
- **Then** the completed sibling is neither reset nor reinvoked because the batch is only partially successful.

## Open decisions

None.

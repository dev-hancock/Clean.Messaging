# INBOX-005 — Atomic local delivery and repeatable execution

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour

**Evidence:** Normative: [coding standards](../../CODING_STANDARDS.md), durable-message consistency and scoped transaction rules, and repository instructions require identity/per-target completion and transaction preservation. Implementation evidence: `InboxDispatcher.Dispatch`, `ConsumerDispatcher.Dispatch`, fenced `DurableStore.Complete`. Reference/explanation pages describe that implementation and add no independent requirement.

Purpose: couple configured-context consumer effects, target completion and newly staged outbox rows. Preconditions: owned entry, resolvable stored consumer/contract, relational scoped configured context. Postcondition: committed success or no committed changes from this local attempt. Invariant: failure/ownership loss cannot authorize a successful local transaction. Boundaries: other contexts and external effects are excluded; execution-strategy retries can reinvoke handlers. This requirement does not promise exactly-once external effects.

## INBOX-005-AC-01 — Commit scoped effects with completion

- **Given** an owned local target whose handler changes the configured scoped context and stages an outbox message.
- **When** local dispatch commits successfully.
- **Then** handler changes, this target completion and staged outbox rows are durable together.

## INBOX-005-AC-02 — Roll back failure and ownership loss

- **Given** a handler has made configured-context changes, but then throws or fenced completion returns false.
- **When** local dispatch exits.
- **Then** the attempt does not commit those changes or outbox rows; an original failure remains a failure and false denotes lost ownership.

## INBOX-005-AC-03 — Recover a committed target without repeating local effects

- **Given** the local transaction committed but the process stopped before the worker observed success.
- **When** a later dispatch checks the retained completed target.
- **Then** it reports completed without repeating its local application effects.

## INBOX-005-AC-04 — Preserve uncertainty without claiming exactly once

- **Given** a commit fails ambiguously or an execution strategy reinvokes a local attempt.
- **When** recovery or strategy retry runs.
- **Then** identity and per-target completion remain the boundary; rollback failure must not replace the original failure, and external side effects are not claimed atomic.

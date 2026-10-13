# Dead letters, retention and observability — Behavioural specification

**Scope:** `DeadLetter`, `RequeueResult`, `IDeadLetterStore<T>.Get/Requeue/Discard`, public EF DeadLetterStore; `IDurableStore<T>.Cleanup`; `MessageMetrics.Completed/Retried/DeadLettered/LeaseLost`. Manager collection semantics and replay are specified in inbox/outbox.

## Purpose
Expose failed durable work for controlled requeue/discard, remove eligible terminal evidence, and report lifecycle observations at their actual boundaries.

## Contracts, preconditions and postconditions
Pre: management targets retained eligible rows; standard mapped stores and configured observation listeners. Post: successful mutations are independently readable; listing is a snapshot, not an audit log. Cleanup changes deduplication evidence and cannot undo external effects.

## Invariants
- Requeue/discard preserve operation identity rather than inventing unrelated work.
- Successful targets are not reset to repair a sibling.
- Metrics/logs represent only their declared local observation boundary.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are unresolved decisions, not executable requirements.

## Limitations
- No all-key atomic manager batch, pagination cursor, payload inspector, exactly-once metrics, universal distributed trace or reversal of discarded external effects.
- Pending rows and inbox admissions have different retention from terminal target entries.

## Implementation trace
These locations explain contract ownership; they do not mandate internal implementation structure.

- [src/Clean.Messaging.Abstractions/DeadLetter.cs](../../../src/Clean.Messaging.Abstractions/DeadLetter.cs)
- [src/Clean.Messaging.Abstractions/RequeueResult.cs](../../../src/Clean.Messaging.Abstractions/RequeueResult.cs)
- [src/Clean.Messaging/DeadLetters/IDeadLetterStore.cs](../../../src/Clean.Messaging/DeadLetters/IDeadLetterStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)
- [src/Clean.Messaging/Processing/MessageMetrics.cs](../../../src/Clean.Messaging/Processing/MessageMetrics.cs)

## Open decisions
The [gap register](../GAPS.md) records unresolved input domains, provider evidence and proposed changes. [INDEX.md](../INDEX.md) explains classification authority and cross-capability ownership.

## Specifications

- [OPS-001 — Dead-letter listing and transitions](OPS-001-dead-letter-listing-and-transitions.md)
- [OPS-002 — Identity-preserving management](OPS-002-identity-preserving-management.md)
- [OPS-003 — Terminal cleanup boundaries](OPS-003-terminal-cleanup-boundaries.md)
- [OPS-004 — Observable metric boundary](OPS-004-observable-metric-boundary.md)
- [OPS-005 — Settlement log attribution](OPS-005-settlement-log-attribution.md)

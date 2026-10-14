# Dead letters, retention and observability capability catalogue

[Specification index](../INDEX.md) · [Documentation index](../../README.md)

## Purpose

Operators need controlled repair of retained failed targets, bounded removal of terminal evidence and measurements with clear local meaning. These capabilities expose lifecycle outcomes; they are not a durable audit stream or exactly-once count of business effects.

## Capability lifecycle

Dead-letter snapshots identify failed targets and their diagnostics. Requeue restores the failed target’s eligibility without replacing its identity or reopening a successful sibling; discard hides it from active dead-letter listing while retaining terminal state until cleanup. Cleanup protects unresolved work and limits each deletion pass. Metrics attribute the observation they name, not a stronger distributed outcome.

## Shared contract boundaries

Management addresses retained per-target work and standard transaction boundaries; a manager collection is not an all-key atomic transaction. Terminal target retention and inbox admission retention are distinct. Deleting completion evidence changes replay/duplicate horizons and cannot reverse external effects. Listeners/exporters and any logger enrichment belong to the host.

The [coding standards](../../CODING_STANDARDS.md) establish ownership, identity, transaction, failure and resource obligations. Intentional public operation and persisted-contract compatibility further constrain these contracts. Reference pages describe implementation; they do not independently approve unresolved policy. Each numbered page separates required outcomes from reserved original obligations.

## Open decisions

[GAP-CORE-007](../GAPS.md#gap-core-007) leaves settlement-log wording undecided. [GAP-DUR-004](../GAPS.md#gap-dur-004) leaves admission retention policy undecided. GAP-INTENT-LIFE-001 reserves which terminal timestamp governs a recently discarded row; its earlier-dead-letter deletion behavior is not approved.

## Specifications

- [OPS-001 — Dead-letter listing and transitions](OPS-001-dead-letter-listing-and-transitions.md)
- [OPS-002 — Identity-preserving management](OPS-002-identity-preserving-management.md)
- [OPS-003 — Terminal cleanup boundaries](OPS-003-terminal-cleanup-boundaries.md)
- [OPS-004 — Observable metric boundary](OPS-004-observable-metric-boundary.md)
- [OPS-005 — Settlement log attribution](OPS-005-settlement-log-attribution.md)

## Implementation trace

These links locate public/provider contracts for lookup; they do not prescribe internal structure.

- [src/Clean.Messaging.Abstractions/DeadLetter.cs](../../../src/Clean.Messaging.Abstractions/DeadLetter.cs)
- [src/Clean.Messaging.Abstractions/RequeueResult.cs](../../../src/Clean.Messaging.Abstractions/RequeueResult.cs)
- [src/Clean.Messaging/DeadLetters/IDeadLetterStore.cs](../../../src/Clean.Messaging/DeadLetters/IDeadLetterStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)
- [src/Clean.Messaging/Processing/MessageMetrics.cs](../../../src/Clean.Messaging/Processing/MessageMetrics.cs)

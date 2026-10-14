# Worker lifecycle and recovery capability catalogue

[Specification index](../INDEX.md) · [Documentation index](../../README.md)

## Purpose

Host authors need durable discovery after missed notifications, bounded local processing and recovery after infrastructure failure. Signals and queues coordinate work; stored per-target state remains its authority.

## Capability lifecycle

A pending wake permits discovery and can coalesce with other hints; it neither stores message identity nor counts completions. Startup and later recovery discover existing eligible durable work without producer restaging. Processing respects local concurrency, preserves escaped failures and disposes owned scopes under cooperative shutdown.

## Shared contract boundaries

The host installs processing/recovery workers, validates budgets and supplies healthy stores/scopes. Lease expiry, retry eligibility and retained evidence constrain recovery. Queue residence does not grant settlement authority, concurrency does not establish FIFO completion, and cooperative cancellation does not forcibly stop user code. No outage-proof deadline, fairness or exactly-once effect is promised.

The [coding standards](../../CODING_STANDARDS.md) establish ownership, identity, transaction, failure and resource obligations. Intentional public operation and persisted-contract compatibility further constrain these contracts. Reference pages describe implementation; they do not independently approve unresolved policy. Each numbered page separates required outcomes from reserved original obligations.

## Open decisions

[GAP-CLAIM-001](../GAPS.md#gap-claim-001) leaves abandoned-claim/attempt-budget policy open. [GAP-WF-003](../GAPS.md#gap-wf-003) reserves ownership protection before queued work is invoked, including the shared-worker backlog boundary; GAP-INTENT-LIFE-003 reserves exact infrastructure backoff. Neither freezes the current queue or delay algorithm.

## Specifications

- [WORK-001 — Durability independent of wake hints](WORK-001-durability-independent-of-wake-hints.md)
- [WORK-002 — Signal observation and cancellation](WORK-002-signal-observation-and-cancellation.md)
- [WORK-003 — Bounded processing and failure recovery](WORK-003-bounded-processing-and-failure-recovery.md)
- [WORK-004 — Queue, leases and producer delay](WORK-004-queue-leases-and-producer-delay.md)

## Implementation trace

These links locate public/provider contracts for lookup; they do not prescribe internal structure.

- [src/Clean.Messaging/Processing/IWorkSignal.cs](../../../src/Clean.Messaging/Processing/IWorkSignal.cs)
- [src/Clean.Messaging/Processing/AsyncAutoResetEvent.cs](../../../src/Clean.Messaging/Processing/AsyncAutoResetEvent.cs)
- [src/Clean.Messaging/Processing/IWorkProcessor.cs](../../../src/Clean.Messaging/Processing/IWorkProcessor.cs)
- [src/Clean.Messaging/Processing/WorkLoop.cs](../../../src/Clean.Messaging/Processing/WorkLoop.cs)
- [src/Clean.Messaging/Processing/WorkLoopOptions.cs](../../../src/Clean.Messaging/Processing/WorkLoopOptions.cs)

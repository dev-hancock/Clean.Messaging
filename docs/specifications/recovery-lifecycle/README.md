# Worker lifecycle, signaling and recovery — Behavioural specification

**Scope:** `IWorkSignal.WaitAsync/Wake`, `AsyncAutoResetEvent.WaitAsync/Set`, `IWorkProcessor<T>.Process`, `WorkLoop<T>.Run`, `WorkLoopOptions.Validate`; feature processing/recovery/cleanup hosted-service contracts are in capability pages.

## Purpose
Find durable outstanding work, bound concurrent local resource use, and recover after lost process-local notifications or failed infrastructure operations.

## Contracts, preconditions and postconditions
Pre: valid positive options, suitable store/processor scopes, running host and cooperative cancellation. Post: durable work remains discoverable despite missed wake hints; bounded local concurrency does not promise complete global ordering or latency.

## Invariants
- A process-local wake is not durable acceptance or publication.
- Active concurrency and owned resources must be bounded and disposed.
- Shutdown must not fabricate successful processing of abandoned claims.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are unresolved decisions, not executable requirements.

## Limitations
- No exact recovery deadline during outages, fairness/global FIFO, queue-age lease renewal or forced interruption of user code.
- Processor escape and infrastructure retry are not themselves persisted per-message policy decisions.

## Implementation trace
These locations explain contract ownership; they do not mandate internal implementation structure.

- [src/Clean.Messaging/Processing/IWorkSignal.cs](../../../src/Clean.Messaging/Processing/IWorkSignal.cs)
- [src/Clean.Messaging/Processing/AsyncAutoResetEvent.cs](../../../src/Clean.Messaging/Processing/AsyncAutoResetEvent.cs)
- [src/Clean.Messaging/Processing/IWorkProcessor.cs](../../../src/Clean.Messaging/Processing/IWorkProcessor.cs)
- [src/Clean.Messaging/Processing/WorkLoop.cs](../../../src/Clean.Messaging/Processing/WorkLoop.cs)
- [src/Clean.Messaging/Processing/WorkLoopOptions.cs](../../../src/Clean.Messaging/Processing/WorkLoopOptions.cs)

## Open decisions
The [gap register](../GAPS.md) records unresolved input domains, provider evidence and proposed changes. [INDEX.md](../INDEX.md) explains classification authority and cross-capability ownership.

## Specifications

- [WORK-001 — Durability independent of wake hints](WORK-001-durability-independent-of-wake-hints.md)
- [WORK-002 — Signal observation and cancellation](WORK-002-signal-observation-and-cancellation.md)
- [WORK-003 — Bounded processing and failure recovery](WORK-003-bounded-processing-and-failure-recovery.md)
- [WORK-004 — Queue, leases and producer delay](WORK-004-queue-leases-and-producer-delay.md)

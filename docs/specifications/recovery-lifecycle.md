# Worker lifecycle, signaling and recovery — Behavioural specification

**Status:** Draft; source reviewed 2026-10-13; no acceptance criterion has been executed.
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
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are counted separately as gaps, not executable requirements.

## Limitations
- No exact recovery deadline during outages, fairness/global FIFO, queue-age lease renewal or forced interruption of user code.
- Processor escape and infrastructure retry are not themselves persisted per-message policy decisions.

## Specifications and acceptance criteria

### WORK-001 — Durability independent of wake hints
**Classification:** Required behaviour
**Evidence:** Coding standards §9 and architecture distinguish durable rows from operational signals; admission/outbox commit and periodic recovery paths.

Pre: committed unresolved work and an operational later host. Post: a lost in-memory signal does not erase accepted work.

#### WORK-001-AC-01 — Missed notification
**Given** durable eligible work exists but its process-local wake was lost.
**When** recovery runs with the host/store available.
**Then** work can be discovered and processed without recreating admission or staging.

#### WORK-001-AC-02 — Restart recovery
**Given** a process stops with unresolved work or abandoned claims.
**When** a later host starts and expired work is eligible.
**Then** the stored work remains available for another attempt.

### WORK-002 — Signal observation and cancellation
**Classification:** Observed behaviour
**Evidence:** AsyncAutoResetEvent has a coalescing one-slot wake; WorkLoop emits initial wake.

Pre: signal can be awaited. Post: a signal permits an observation without promising a counter per Wake.

#### WORK-002-AC-01 — Wake before wait
**Given** one wake has been posted with no active waiter.
**When** WaitAsync runs.
**Then** the pending wake permits the wait to complete.

#### WORK-002-AC-02 — Wake coalescing
**Given** many Set calls occur before a waiter consumes one.
**When** waits are observed.
**Then** the signal is a hint and does not require one completion per Set; no work identity is stored by the signal.

#### WORK-002-AC-03 — Cancelled empty wait
**Given** no wake is pending and the wait token is cancelled.
**When** WaitAsync waits.
**Then** it ends with cancellation rather than reporting that durable work was processed.

#### WORK-002-AC-04 — Initial discovery
**Given** eligible stored work exists at host start with no producer notification.
**When** WorkLoop.Run starts normally.
**Then** it requests initial discovery instead of requiring a new external enqueue.

### WORK-003 — Bounded processing and failure recovery
**Classification:** Required behaviour
**Evidence:** Coding standards §16 requires bounded resources; §7 permits deliberate recovery with original failure semantics.

Pre: configured capacity/concurrency and failing/recovering dependencies. Post: failed infrastructure work does not become falsely completed; processing respects the configured local concurrency budget.

#### WORK-003-AC-01 — Concurrency bound
**Given** more queued targets than configured Concurrency and handlers are deliberately held.
**When** work runs.
**Then** no more than the configured number of local processing calls overlap.

#### WORK-003-AC-02 — Claim outage recovery
**Given** the store first fails claiming then becomes available.
**When** the live loop continues and its recovery delay passes.
**Then** claiming is retried without claiming that unavailable work already completed.

#### WORK-003-AC-03 — Escaped processor failure
**Given** one processing call throws unexpectedly and other stored work remains.
**When** the loop handles the exception.
**Then** the failure is diagnostic and subsequent work can continue; the failed target is not automatically marked completed.

#### WORK-003-AC-04 — Cooperative shutdown
**Given** waiting/processing dependencies honor cancellation.
**When** the host stops.
**Then** the loop terminates and disposes owned operation scopes without claiming abandoned targets succeeded.

### WORK-004 — Queue, leases and producer delay
**Classification:** Observed behaviour
**Evidence:** WorkLoop claims before enqueueing and renews only through processors; producer uses capped exponential infrastructure delay.

Pre: standard shared work loop, capacity/concurrency/batch valid. Post: deterministic claim selection does not imply ordered finish or fresh queued leases.

#### WORK-004-AC-01 — Queued lease expiry
**Given** one claimed target waits behind a held handler longer than its lease.
**When** its processing slot becomes available.
**Then** the queue did not independently renew that target; a completion still requires valid current ownership.

#### WORK-004-AC-02 — Producer delay budget
**Given** successive claim calls fail and a later call succeeds.
**When** recovery time is controlled.
**Then** delay begins at RecoveryInterval, doubles with failures up to FailureMaxDelay, and successful claiming resets that failure sequence.

#### WORK-004-AC-03 — Invalid loop options
**Given** Capacity/Concurrency/BatchSize <=0 or a loop time value <=0.
**When** Run/Validate is requested.
**Then** validation fails before normal processing begins.

#### WORK-004-AC-04 — No FIFO completion guarantee
**Given** several targets are claimed in selection order but handlers finish at different speeds.
**When** concurrent processing completes.
**Then** no invariant requires completion to follow that selection order.

## Traceability and verification matrix
All rows are **Not tested**. Unit means isolated observable contract verification, not internal-call assertions. Persistence/concurrency verification requires a suitable real relational provider, not EF InMemory. The eventual TUnit suite can combine criteria while retaining their IDs.

| Specification | Criterion | Classification | Verification | Observable assertion | Status |
| --- | --- | --- | --- | --- | --- |
| WORK-001 | WORK-001-AC-01 | Required behaviour | Workflow integration | Missed notification | Not tested |
| WORK-001 | WORK-001-AC-02 | Required behaviour | Workflow integration | Restart recovery | Not tested |
| WORK-002 | WORK-002-AC-01 | Observed behaviour | Unit | Wake before wait | Not tested |
| WORK-002 | WORK-002-AC-02 | Observed behaviour | Unit | Wake coalescing | Not tested |
| WORK-002 | WORK-002-AC-03 | Observed behaviour | Unit | Cancelled empty wait | Not tested |
| WORK-002 | WORK-002-AC-04 | Observed behaviour | Integration | Initial discovery | Not tested |
| WORK-003 | WORK-003-AC-01 | Required behaviour | Concurrency integration | Concurrency bound | Not tested |
| WORK-003 | WORK-003-AC-02 | Required behaviour | Integration | Claim outage recovery | Not tested |
| WORK-003 | WORK-003-AC-03 | Required behaviour | Integration | Escaped processor failure | Not tested |
| WORK-003 | WORK-003-AC-04 | Required behaviour | Integration | Cooperative shutdown | Not tested |
| WORK-004 | WORK-004-AC-01 | Observed behaviour | Concurrency integration | Queued lease expiry | Not tested |
| WORK-004 | WORK-004-AC-02 | Observed behaviour | Integration | Producer delay budget | Not tested |
| WORK-004 | WORK-004-AC-03 | Observed behaviour | Unit | Invalid loop options | Not tested |
| WORK-004 | WORK-004-AC-04 | Observed behaviour | Concurrency integration | No FIFO completion guarantee | Not tested |

## Implementation trace
These locations are evidence, not mandated internal architecture. The full inspected-file/public-surface matrix is in [INSPECTION.md](INSPECTION.md).

- [src/Clean.Messaging/Processing/IWorkSignal.cs](../../src/Clean.Messaging/Processing/IWorkSignal.cs)
- [src/Clean.Messaging/Processing/AsyncAutoResetEvent.cs](../../src/Clean.Messaging/Processing/AsyncAutoResetEvent.cs)
- [src/Clean.Messaging/Processing/IWorkProcessor.cs](../../src/Clean.Messaging/Processing/IWorkProcessor.cs)
- [src/Clean.Messaging/Processing/WorkLoop.cs](../../src/Clean.Messaging/Processing/WorkLoop.cs)
- [src/Clean.Messaging/Processing/WorkLoopOptions.cs](../../src/Clean.Messaging/Processing/WorkLoopOptions.cs)

## Open decisions and evidence limits
The [gap register](GAPS.md) records unresolved input domains, provider evidence and proposed changes. Source inspection is not execution proof. [INDEX.md](INDEX.md) explains classification authority and cross-capability ownership.

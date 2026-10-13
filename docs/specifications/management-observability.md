# Dead letters, retention and observability — Behavioural specification

**Status:** Draft; source reviewed 2026-10-13; no acceptance criterion has been executed.
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
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are counted separately as gaps, not executable requirements.

## Limitations
- No all-key atomic manager batch, pagination cursor, payload inspector, exactly-once metrics, universal distributed trace or reversal of discarded external effects.
- Pending rows and inbox admissions have different retention from terminal target entries.

## Specifications and acceptance criteria

### OPS-001 — Dead-letter listing and transitions
**Classification:** Observed behaviour
**Evidence:** DeadLetterStore lists non-discarded dead letters and mutates only that state; public snapshot fields.

Pre: retained non-discarded dead letter. Post: requeue resets attempt budget/eligibility while retaining identity/failure; discard hides listing but does not immediately delete.

#### OPS-001-AC-01 — List projection
**Given** dead letters, discarded and successful entries exist.
**When** positive limited listing runs.
**Then** only non-discarded dead letters appear with Key/EventId/time/Attempts/Code/Error, ordered by dead-letter time without a tie-order promise.

#### OPS-001-AC-02 — Requeue reset
**Given** a retained non-discarded dead letter has previous failure diagnostics.
**When** Requeue succeeds.
**Then** it becomes unresolved/immediately eligible with Attempts zero, cleared ownership/dead-letter status and unchanged identity/payload/failure history.

#### OPS-001-AC-03 — Discard visibility
**Given** a non-discarded dead letter.
**When** Discard succeeds.
**Then** it is absent from dead-letter listing and retains a stored discarded outcome until cleanup.

#### OPS-001-AC-04 — Ineligible mutation
**Given** a key is absent, already discarded or successful.
**When** Requeue or Discard runs.
**Then** the normal operation returns false and does not reset successful work.

#### OPS-001-AC-05 — Independent mutation restriction
**Given** an ambient TransactionScope is active.
**When** standard Requeue or Discard is requested.
**Then** InvalidOperationException rejects the unsupported ambient boundary rather than silently suppressing it.

### OPS-002 — Identity-preserving management
**Classification:** Required behaviour
**Evidence:** Coding standards §9 requires preserving retry identity and independently successful targets.

Pre: multiple targets for one logical event, one successful and one dead letter. Post: repairing the failed target does not rewrite successful siblings or turn the same event into a new event.

#### OPS-002-AC-01 — Repair one target
**Given** target A completed and B dead-lettered for the same event.
**When** B is requeued.
**Then** A remains completed and B retains its original message/event/target identity.

### OPS-003 — Terminal cleanup boundaries
**Classification:** Observed behaviour
**Evidence:** DurableStore deletes a limited set with any terminal timestamp strictly older than cutoff; inbox admissions are not this generic entry type.

Pre: positive limit and terminal-retention cutoff. Post: bounded old terminal rows disappear; unresolved and boundary-equal rows are retained.

#### OPS-003-AC-01 — Strict cutoff
**Given** terminal timestamps are older than, equal to and newer than cutoff.
**When** Cleanup runs.
**Then** only strictly older qualifying terminal rows can be deleted.

#### OPS-003-AC-02 — Pending protection
**Given** an unresolved pending/retrying/claimed entry has very old CreatedAt.
**When** Cleanup runs.
**Then** creation age alone does not delete it.

#### OPS-003-AC-03 — Bounded deletion
**Given** more eligible terminal entries than limit.
**When** one Cleanup runs.
**Then** the reported deletion count is at most limit; a pass is not promised to drain all eligible rows.

#### OPS-003-AC-04 — Discard age
**Given** a recently discarded row has an older dead-letter timestamp before cutoff.
**When** Cleanup runs.
**Then** it may be removed based on that older terminal timestamp rather than restarting retention at discard.

#### OPS-003-AC-05 — Evidence removal
**Given** a completed target is cleaned while its inbox admission is retained.
**When** ordinary duplicate Accept and explicit replay are later attempted.
**Then** admission deduplication and missing-target replay differ; cleanup did not imply the admission was deleted.

### OPS-004 — Observable metric boundary
**Classification:** Observed behaviour
**Evidence:** MessageMetrics counters are named and tagged public output; processors increment retry/dead-letter only after true settlement.

Pre: a listener subscribes to the Clean.Messaging meter. Post: public methods increment their named counter with source, not proof of remote business success.

#### OPS-004-AC-01 — Named counters
**Given** a listener observes the meter and a public metric operation is invoked with source X.
**When** Completed/Retried/DeadLettered/LeaseLost records an observation.
**Then** respectively messaging.completed/retries/dead_letters/lease_losses increments by one with source X.

#### OPS-004-AC-02 — Rejected settlement metrics
**Given** retry/dead-letter settlement returns false in a processing attempt.
**When** metrics are observed.
**Then** no successful retry/dead-letter increment is reported for that rejected settlement; claim loss is observed instead.

#### OPS-004-AC-03 — Completion observation
**Given** a local dispatcher short-circuits an already completed entry.
**When** processor observes a successful dispatcher result.
**Then** a completion observation can occur without a new database transition; the counter is not exactly-once business-effect count.

#### OPS-004-AC-04 — No automatic exporter
**Given** only library services are registered.
**When** a process starts without application listeners/exporters.
**Then** no claim is made that metrics are externally collected or that correlation becomes an automatic logger scope.

### OPS-005 — Settlement log attribution
**Classification:** Unknown behaviour
**Evidence:** Processor retry/dead-letter wording is logged before conditional persistence; outcome-looking log may precede a rejected/failing update.

Unknown: whether settlement-intent and confirmed settlement should have distinct operational messages. Current metric boundary differs from logs; proposal belongs in GAPS, not silent telemetry rewrite.

**Open decisions:** [GAP-CORE-007](GAPS.md#gap-core-007).

## Traceability and verification matrix
All rows are **Not tested**. Unit means isolated observable contract verification, not internal-call assertions. Persistence/concurrency verification requires a suitable real relational provider, not EF InMemory. The eventual TUnit suite can combine criteria while retaining their IDs.

| Specification | Criterion | Classification | Verification | Observable assertion | Status |
| --- | --- | --- | --- | --- | --- |
| OPS-001 | OPS-001-AC-01 | Observed behaviour | Persistence integration | List projection | Not tested |
| OPS-001 | OPS-001-AC-02 | Observed behaviour | Persistence integration | Requeue reset | Not tested |
| OPS-001 | OPS-001-AC-03 | Observed behaviour | Persistence integration | Discard visibility | Not tested |
| OPS-001 | OPS-001-AC-04 | Observed behaviour | Persistence integration | Ineligible mutation | Not tested |
| OPS-001 | OPS-001-AC-05 | Observed behaviour | Persistence integration | Independent mutation restriction | Not tested |
| OPS-002 | OPS-002-AC-01 | Required behaviour | Workflow integration | Repair one target | Not tested |
| OPS-003 | OPS-003-AC-01 | Observed behaviour | Persistence integration | Strict cutoff | Not tested |
| OPS-003 | OPS-003-AC-02 | Observed behaviour | Persistence integration | Pending protection | Not tested |
| OPS-003 | OPS-003-AC-03 | Observed behaviour | Persistence integration | Bounded deletion | Not tested |
| OPS-003 | OPS-003-AC-04 | Observed behaviour | Persistence integration | Discard age | Not tested |
| OPS-003 | OPS-003-AC-05 | Observed behaviour | Workflow integration | Evidence removal | Not tested |
| OPS-004 | OPS-004-AC-01 | Observed behaviour | Unit | Named counters | Not tested |
| OPS-004 | OPS-004-AC-02 | Observed behaviour | Integration | Rejected settlement metrics | Not tested |
| OPS-004 | OPS-004-AC-03 | Observed behaviour | Integration | Completion observation | Not tested |
| OPS-004 | OPS-004-AC-04 | Observed behaviour | Unit | No automatic exporter | Not tested |

## Implementation trace
These locations are evidence, not mandated internal architecture. The full inspected-file/public-surface matrix is in [INSPECTION.md](INSPECTION.md).

- [src/Clean.Messaging.Abstractions/DeadLetter.cs](../../src/Clean.Messaging.Abstractions/DeadLetter.cs)
- [src/Clean.Messaging.Abstractions/RequeueResult.cs](../../src/Clean.Messaging.Abstractions/RequeueResult.cs)
- [src/Clean.Messaging/DeadLetters/IDeadLetterStore.cs](../../src/Clean.Messaging/DeadLetters/IDeadLetterStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DeadLetterStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)
- [src/Clean.Messaging/Processing/MessageMetrics.cs](../../src/Clean.Messaging/Processing/MessageMetrics.cs)

## Open decisions and evidence limits
The [gap register](GAPS.md) records unresolved input domains, provider evidence and proposed changes. Source inspection is not execution proof. [INDEX.md](INDEX.md) explains classification authority and cross-capability ownership.

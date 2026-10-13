# SCHED-013 — Local wake engine, commit hints and polling recovery

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** ISchedulingEngine.Wake/Wait(DateTime,token) supply local hints, not durable delivery. Save interception covers sync/async observed saves and explicit transaction outcomes. Recovery also polls.

**Evidence:** Runtime/SchedulingEngine.cs; SchedulingWorker.cs; SchedulingTracker.cs; SchedulingSaveInterceptor.cs; SchedulingTransactionInterceptor.cs.

**Intent gaps:** [GAP-WF-006](../GAPS.md#gap-wf-006).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-013-AC-01 — Time and local hint both release wait

**Given** a future wake instant and no cancellation.
**When** time reaches the instant or a local Wake arrives first.
**Then** Wait returns to permit re-evaluation, without itself completing or claiming any durable schedule.

## SCHED-013-AC-02 — Past wait and cancelled future wait

**Given** a past instant or a cancelled token for a future wait.
**When** Wait is called.
**Then** past wait returns without delay even if the token is already cancelled; future waiting observes cancellation rather than a false delivery success.

## SCHED-013-AC-03 — Committed saves cause reevaluation

**Given** changed schedules are saved synchronously or asynchronously with implicit or explicit ownership.
**When** standalone save succeeds or explicit commit finishes.
**Then** due processing can re-evaluate durable work; an explicit save alone does not establish commit.

## SCHED-013-AC-04 — Missed hints are eventually rechecked

**Given** a schedule commits without a usable local wake or belongs to another host.
**When** polling and recovery continue with storage available.
**Then** eligible work is revisited using due/retry eligibility and recovery interval, not an indefinite wait for one lost notification.

## SCHED-013-AC-05 — Wake after failure is only a hint

**Given** save cancellation/failure or transaction failure produced a wake.
**When** worker re-evaluates storage.
**Then** it processes only actually committed eligible rows; waking is not proof of rollback or successful uncertain commit.

## SCHED-013-AC-06 — Nested saves and ambient changed-work transactions are rejected

**Given** changed schedules are saved inside an ambient transaction or a nested observed save.
**When** save observation begins.
**Then** unsupported ownership is rejected instead of promising atomic processing; unrelated saves without changed schedules are not rejected by this scheduling check.

## SCHED-013-AC-07 — Infrastructure fault does not abandon worker forever

**Given** claim/query/dispatch infrastructure raises an error.
**When** host remains running and storage recovers.
**Then** the worker can reevaluate after its recovery delay; fault delay does not terminally mark every schedule.

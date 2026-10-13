# SCHED-009 — Due eligibility, claims and bounded execution

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** Committed active schedules become claimable at due/retry and lease boundaries. Attempts measures claims, not invocations. Processing.Concurrency bounds admitted delivery work within one worker; Recovery.BatchSize bounds selection, not global work.

**Evidence:** ScheduleStore.NextDue/Claim; SchedulingWorker.cs; OwnedSchedule.cs; SchedulingOptions.cs; ScheduleExecutor.cs.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-009-AC-01 — All eligibility predicates hold

**Given** rows differ by terminal status, original due, retry due and lease expiry.
**When** claim runs at time T.
**Then** only nonterminal rows with DueAt <= T, retry absent or <= T, and lease absent or <= T become owned.

## SCHED-009-AC-02 — Boundary instant is claimable

**Given** eligible rows have due/retry/old lease exactly T.
**When** claim runs at T.
**Then** equality permits claiming; a timestamp just after T blocks the corresponding predicate.

## SCHED-009-AC-03 — Batch selection is bounded and ordered

**Given** more eligible schedules than batch size including equal due instants.
**When** claim completes.
**Then** no more than the configured batch is owned, ordered by original due then identity; nonselected rows remain available.

## SCHED-009-AC-04 — Claim increases attempts before delivery admission

**Given** a selected schedule waits behind busy execution slots.
**When** the claim is committed but delivery has not started.
**Then** Attempts has already increased; the counter does not purport to count handler calls or EF strategy repeats.

## SCHED-009-AC-05 — Competing claims do not share ownership

**Given** two workers claim overlapping eligible rows.
**When** claim transactions complete.
**Then** committed ownership fencing distinguishes current attempts; an old token cannot settle a replacement token.

## SCHED-009-AC-06 — Recovery is not a hard maximum invocation budget

**Given** a row repeatedly expires or processing is cancelled without failure settlement and its Attempts already reaches MaxAttempts.
**When** it becomes eligible again.
**Then** it can be claimed again; MaxAttempts gates failure disposition, not universal claim eligibility.

## SCHED-009-AC-07 — Execution parallelism has local scope

**Given** a worker has more claimed rows than configured concurrency.
**When** deliveries are pending.
**Then** at most that worker concurrency are admitted together; completion need not follow due order, and multiple hosts are not a global semaphore.

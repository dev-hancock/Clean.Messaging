# SCHED-016 — Terminal retention, recovery boundaries and management limits

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** Cleanup removes old terminal schedules, not active overdue work. There is no public schedule upsert, recurring schedule, requeue/dead-letter repair, failed-work restart or lease-renewal API. Read/cancel are not repair commands.

**Evidence:** ScheduleStore.Cleanup; ScheduleCleanupWorker.cs; SchedulingOptions.Cleanup; all owned public scheduling surfaces.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-016-AC-01 — Strict terminal retention cutoff

**Given** dispatched/cancelled/failed rows before, equal to and after cutoff plus very old active work.
**When** cleanup completes.
**Then** only terminal rows with terminal timestamp strictly before cutoff are deleted; equal/after and active rows remain.

## SCHED-016-AC-02 — Cleanup is periodic and bounded

**Given** more eligible rows than batch times cap.
**When** a cleanup tick runs.
**Then** deletion uses batches ordered by CreatedAt, stops at configured per-run cap with warning when capped, and leaves remaining work for later ticks.

## SCHED-016-AC-03 — Cleanup recovery and initial timing

**Given** a host starts or a cleanup query fails.
**When** periodic cleanup lifecycle continues.
**Then** no immediate startup purge is assumed; failed cleanup can retry on a later tick without changing active schedules into terminal ones.

## SCHED-016-AC-04 — Deletion removes identity protection and history

**Given** an old terminal schedule is physically removed.
**When** the same identity is inserted later.
**Then** the deleted schedule row no longer prevents reuse or supplies history; retained outgoing/timer evidence has its own independent lifetime.

## SCHED-016-AC-05 — Cancellation is not failure requeue

**Given** a retained failed schedule is read or cancelled.
**When** management APIs complete.
**Then** it remains failed and is not automatically reopened, rescheduled or compensated.

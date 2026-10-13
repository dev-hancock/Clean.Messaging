# SCHED-007 — Cancellation filters, terminal states and races

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** IMessageScheduler.Cancel/CancelGroup use message target; IScheduleWriter.Cancel has optional group, CancelGroup has exceptId. Cancellation stages changes and does not revoke external effects.

**Evidence:** OutboxMessageScheduler.cs; Runtime/ScheduleWriter.cs; ScheduleStore.Cancel/CancelGroup; ScheduleEntry.Cancel; SchedulingConfiguration.cs.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-007-AC-01 — Single cancellation respects target and optional group

**Given** active schedule X with a particular target/group.
**When** Cancel uses a different target or a nonmatching supplied group.
**Then** X remains active; a matching command followed by save/commit records cancellation.

## SCHED-007-AC-02 — Public group cancellation is message-only

**Given** one group contains message and saga target schedules.
**When** IMessageScheduler.CancelGroup commits.
**Then** only matching active message-target schedules cancel; saga/other groups remain unchanged.

## SCHED-007-AC-03 — Tracked and persisted group work are included

**Given** a group has unsaved new/modified and persisted active schedules plus terminal rows.
**When** a matching group cancel excluding X commits.
**Then** all matching nonterminal schedules except X cancel, including newly staged ones; terminal rows do not transition.

## SCHED-007-AC-04 — Missing and terminal cancellation are no-ops

**Given** an absent schedule or an already dispatched/cancelled/failed row.
**When** matching single or group cancellation completes.
**Then** no replacement row, reopened work or altered terminal outcome is created.

## SCHED-007-AC-05 — Cancellation removes retry ownership without erasing diagnostics

**Given** an active claimed/retrying schedule has diagnostics.
**When** cancellation commits.
**Then** CancelledAt is present, claim/lease/retry eligibility is cleared, and existing diagnostics remain visible.

## SCHED-007-AC-06 — Concurrent cancellation does not reverse delivery

**Given** processing overlaps cancellation of the same schedule.
**When** their transactions compete.
**Then** a stale write may fail optimistic concurrency; the winning terminal result is not undone, and already performed external effects cannot be recalled.

## SCHED-007-AC-07 — Cancel obeys owner rollback and cancellation token

**Given** a cancellation is staged or its database query receives a cancelled token.
**When** the owner rolls back or I/O is cancelled.
**Then** rolled-back cancellation is not durable; cancellation is propagated rather than reported as successful terminal mutation.

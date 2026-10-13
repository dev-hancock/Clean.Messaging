# SCHED-006 — Staging, duplicate insertion and owner visibility

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** Schedule stages work, not an immediate durable insert/publication. The EF mapping keys schedules by ScheduleId and marks Version for concurrency. Owner SaveChanges and transaction commit define durability.

**Evidence:** Runtime/ScheduleWriter.cs; ScheduleStore.Add; SchedulingConfiguration.cs; SchedulingTracker.cs; save/transaction interceptors; shared owner dispatchers.

**Intent gaps:** [GAP-WF-002](../GAPS.md#gap-wf-002), [GAP-WF-006](../GAPS.md#gap-wf-006).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-006-AC-01 — Staged schedule requires save

**Given** a valid unsaved schedule.
**When** a separate persisted read or due worker queries.
**Then** the staged schedule is not visible as durable work until the owner saves and commits.

## SCHED-006-AC-02 — Explicit transaction delays durable visibility

**Given** the owner saved a new schedule inside a user transaction.
**When** the transaction is still open and later rolls back.
**Then** other committed readers do not observe a successful new schedule and rollback leaves no durable new operation.

## SCHED-006-AC-03 — Standalone save and explicit commit publish durable work

**Given** a valid schedule in an implicit save or explicit transaction.
**When** the save succeeds or the explicit transaction commits.
**Then** the operation becomes available for due processing after its committed due instant.

## SCHED-006-AC-04 — Repeated insert is not an upsert

**Given** a retained schedule with ID X and a retry using ID X with equal or changed payload/due/group.
**When** the retry is staged and saved.
**Then** the existing row is not replaced or rescheduled; duplicate tracked/database identity can reject the attempt rather than return idempotent success.

## SCHED-006-AC-05 — Duplicate insert race preserves one identity

**Given** two contexts stage the same ScheduleId.
**When** they compete to commit.
**Then** at most one row with that identity is committed; the losing insertion is not guaranteed a domain-specific duplicate result.

## SCHED-006-AC-06 — Save failure cannot establish successful staging

**Given** serialization succeeded but database save/commit fails or is cancelled.
**When** the owner handles the failure.
**Then** a wake hint is not evidence of a durable schedule; only independently established committed state is processed.

# SCHED-008 — Persisted management snapshots

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** IScheduleReader.GetGroup(groupId, target?, token) returns ScheduleStatus snapshots: ScheduleId/ScheduleGroupId/Target/MessageData/due/attempts/retry/three terminal timestamps/code/message. Claims, lease, version and exception type are not management fields.

**Evidence:** IScheduleReader.cs; ScheduleStatus.cs; ScheduleStore.GetGroup.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-008-AC-01 — Group snapshots include terminal history

**Given** mixed active and terminal rows in two groups and targets.
**When** GetGroup reads with a target or no target.
**Then** only the requested group is returned, target filter is exact when supplied, all targets when omitted, and terminal rows remain included.

## SCHED-008-AC-02 — Ordering is stable for equal due instants

**Given** group rows with differing due times and IDs including equal due times.
**When** GetGroup completes.
**Then** snapshots are ordered by due instant then schedule identity rather than completion or insertion order.

## SCHED-008-AC-03 — Read does not expose unsaved mutations

**Given** a context has an unsaved addition/cancellation and persisted rows.
**When** GetGroup is used.
**Then** the result describes persisted state, not an in-memory merge, and changing snapshot data does not mutate stored lifecycle state.

## SCHED-008-AC-04 — Empty group and read I/O failures

**Given** a group is absent or storage is unavailable/cancelled.
**When** GetGroup is used.
**Then** absent group returns an empty list; storage failure/cancellation propagates rather than being an empty successful read.

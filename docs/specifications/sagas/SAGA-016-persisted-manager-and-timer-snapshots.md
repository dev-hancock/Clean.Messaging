# SAGA-016 — Persisted manager and timer snapshots

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** ISagaManager.Get(id,token), GetActive(limit=100,token), GetFailed(limit=100,token), GetTimers(id,token). Saga snapshot exposes Id/Type/Key/Version/timestamps/code/message and IsActive/IsTerminal, not state/start identity. SagaTimer exposes ID/owner/type/contract/due/attempt/retry/consumed/cancelled/failed/code/message, not payload/ownership metadata.

**Evidence:** ISagaManager.cs; Saga.cs; SagaTimer.cs; SagaManager.cs; SagaStore.Get/GetActive/GetFailed; ScheduleStore.GetGroup.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-016-AC-01 — Get and absent timer listing are nonmutating

**Given** persisted active/terminal/absent SagaId.
**When** Get or GetTimers completes.
**Then** Get returns the matching snapshot or null; absent saga yields no timers; reads do not reopen/create/purge work.

## SAGA-016-AC-02 — Lists filter and bound persisted state

**Given** mixed active/complete/failed rows and positive requested limit.
**When** GetActive or GetFailed reads.
**Then** active list excludes all terminal rows and orders CreatedAt ascending; failed list excludes active/complete and orders FailedAt descending; no more than limit are returned, ties unspecified.

## SAGA-016-AC-03 — Limits reject nonpositive values

**Given** limit zero or negative versus omitted or positive.
**When** GetActive/GetFailed is called.
**Then** nonpositive limit rejects; omitted limit is 100; positive limit bounds persisted results.

## SAGA-016-AC-04 — Timer projection uses saga target and history

**Given** a saga has active and terminal timers plus other-target schedules.
**When** GetTimers reads.
**Then** only saga group/target timers appear ordered due then ID, ConsumedAt reflects schedule dispatch, and IsTerminal is true for consumed/cancelled/failed timestamps.

## SAGA-016-AC-05 — Snapshot flags and unsaved changes

**Given** persisted rows have completion/failure/null terminal timestamps while local changes remain unsaved.
**When** manager reads snapshots.
**Then** IsActive iff both saga terminal timestamps are absent, IsTerminal is inverse; persisted values are returned without unsaved-state merge or mutable tracked state exposure.

## SAGA-016-AC-06 — Read failure and cancellation are not empty success

**Given** manager database I/O fails or is cancelled.
**When** Get/list/timer read runs.
**Then** failure or cancellation propagates; only genuinely absent persisted data becomes null/empty.

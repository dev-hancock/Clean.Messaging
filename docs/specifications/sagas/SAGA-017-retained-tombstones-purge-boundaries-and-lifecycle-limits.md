# SAGA-017 — Retained tombstones, purge boundaries and lifecycle limits

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** Saga cleanup purges JSON state of old terminal records, not identity/tombstones. Scheduling retention independently deletes timer rows. There is no public reopen/reset/force-result/replay/identity-delete/state-migration or stuck-workflow repair command.

**Evidence:** SagaStore.PurgeState; SagaCleanupWorker.cs; SagaEntry.cs; SagaConfiguration.cs; ISagaManager.cs; Scheduling cleanup.

**Intent gaps:** [GAP-WF-011](../GAPS.md#gap-wf-011).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-017-AC-01 — Strict terminal purge cutoff

**Given** completed/failed rows with terminal timestamp before/equal/after cutoff and old active rows.
**When** a purge batch runs.
**Then** only previously unpurged terminal state strictly before cutoff is replaced with JSON null; equality/after/active rows retain their state.

## SAGA-017-AC-02 — Purge retains identity and lifecycle diagnostics

**Given** an eligible terminal saga is purged.
**When** persisted snapshot and uniqueness are read.
**Then** StatePurgedAt records purge time; type/key/start identity/outcome/diagnostics remain, and Version and UpdatedAt are not incremented by purge.

## SAGA-017-AC-03 — Purge is periodic bounded and recoverable

**Given** host starts or purge fails with more eligible rows than batch.
**When** cleanup lifecycle continues.
**Then** no immediate startup purge is assumed; one configured batch ordered by UpdatedAt is attempted per tick; failure retries on later tick.

## SAGA-017-AC-04 — Tombstone blocks key restart indefinitely today

**Given** a terminal tombstone survives far beyond retention.
**When** same start, different start or continuation arrives.
**Then** same start remains no-op, different start remains closed and continuation remains ignored; retention does not free that type/key.

## SAGA-017-AC-05 — Timer history has independent retention

**Given** saga tombstone survives while old terminal timer rows are deleted.
**When** GetTimers and Get are read.
**Then** saga identity/outcome can remain while timer history is absent; this is not a complete indefinite workflow audit trail.

## SAGA-017-AC-06 — Active failed-timer workflow needs application policy

**Given** saga remains active with failed/exhausted timers and no later business message.
**When** cleanup and manager reads occur.
**Then** active saga is not automatically purged, failed, compensated or repaired; manager APIs provide observation, not a business policy engine.

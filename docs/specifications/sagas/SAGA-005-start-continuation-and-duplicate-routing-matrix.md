# SAGA-005 — Start, continuation and duplicate routing matrix

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** Start is defined by correlate+create. StartedByMessageId identifies the original incoming delivery, not EventId. Continuations have no per-saga consumed-message ledger.

**Evidence:** SagaMessageDefinition.cs; SagaRuntime.Create/Find; SagaProcessor.cs; SagaEntry.cs; SagaConsumer.cs.

**Intent gaps:** [GAP-WF-012](../GAPS.md#gap-wf-012).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-005-AC-01 — Absent start creates initial state

**Given** no persisted type/key and a nonnull state factory result.
**When** a start is handled and owner commits.
**Then** a new SagaId, the incoming MessageId as start identity, initial timestamps and handler-produced state/outcome are retained.

## SAGA-005-AC-02 — Same start identity is replay no-op

**Given** active or terminal saga has StartedByMessageId X.
**When** start message X is handled again.
**Then** no new saga/state transition/outgoing effects are committed, including when terminal state was purged.

## SAGA-005-AC-03 — Different active start conflicts

**Given** active saga at type/key was started by X.
**When** start Y != X arrives.
**Then** saga.start.conflict is permanent and exposes type/key; the existing workflow is not overwritten.

## SAGA-005-AC-04 — Different terminal start is closed

**Given** completed/failed saga at type/key was started by X.
**When** start Y != X arrives.
**Then** saga.start.closed is permanent; completion/failure is not reopened.

## SAGA-005-AC-05 — Absent continuation is transient missing

**Given** no persisted saga at the correlated type/key.
**When** continuation arrives.
**Then** saga.not_found is retry-classified with no new saga or effects, allowing a later committed start to make a retry meaningful.

## SAGA-005-AC-06 — Active continuation can transition

**Given** a persisted active saga exists.
**When** a continuation handles state and owner commits.
**Then** state/outcome/effects reflect that handler result and the persisted version advances.

## SAGA-005-AC-07 — Terminal continuation is ignored

**Given** a terminal saga exists, possibly with purged JSON state.
**When** continuation arrives.
**Then** no handler state transition/effects or reopening is committed.

## SAGA-005-AC-08 — General duplicate continuation belongs to owner deduplication

**Given** an active saga receives the same continuation in separate eligible owner executions.
**When** both executions are admitted.
**Then** no per-saga message ledger guarantees suppression; inbox/outbox owner identity and fencing, not the saga key alone, govern duplicate processing.

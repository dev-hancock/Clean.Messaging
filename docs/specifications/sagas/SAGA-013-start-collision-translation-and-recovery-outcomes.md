# SAGA-013 — Start collision translation and recovery outcomes

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** Save-failure translation consults a fresh persisted winner for exactly one distinct Added saga identity in DbUpdateException. Same original start becomes retry-classified saga.start.race; different active/terminal winner becomes Conflict/Closed. It is not a generic upsert or blanket database error conversion.

**Evidence:** SagaConflictResolver.cs; ISagaConflictResolver.cs (internal); SagaRaceException.cs; SagaSaveInterceptor.cs; SagaConfiguration.cs.

**Intent gaps:** [GAP-WF-009](../GAPS.md#gap-wf-009), [GAP-WF-010](../GAPS.md#gap-wf-010).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-013-AC-01 — Same-start insertion race is transient

**Given** two contexts insert the same type/key/StartedByMessageId and one commits.
**When** loser save fails with one qualifying added identity and finds winner.
**Then** failure is translated to retry-classified saga.start.race; a later replay sees the retained start and is a no-op.

## SAGA-013-AC-02 — Different-start active race is permanent conflict

**Given** two different start identities compete for the same type/key.
**When** loser resolves an active persisted winner.
**Then** saga.start.conflict is permanent and exposes the winning type/key boundary without replacing its state.

## SAGA-013-AC-03 — Winner terminal race is permanent closed

**Given** different start lost to a now-terminal persisted workflow.
**When** fresh conflict lookup succeeds.
**Then** saga.start.closed is permanent, not reopening or an idempotent acknowledgement of the different start.

## SAGA-013-AC-04 — Nonqualifying save failures retain original error

**Given** error is not DbUpdateException, has zero/multiple distinct Added starts, has no persisted winner or lookup itself fails.
**When** translation is considered.
**Then** the original failure is preserved rather than fabricating conflict/closed/race without qualifying evidence.

## SAGA-013-AC-05 — Sync and async collision paths agree

**Given** equivalent qualifying insertion races via SaveChanges and SaveChangesAsync.
**When** winner lookup succeeds.
**Then** both expose the same race/conflict/closed classification for the same retained start relationship.

## SAGA-013-AC-06 — Public exception classification is compatible

**Given** missing saga by ID or type/key, conflict, closed and serialization exception are constructed.
**When** their public Code/Action/IsTransient and identity fields are read.
**Then** as a candidate compatibility baseline, missing is saga.not_found/Retry, conflict saga.start.conflict/Fault, closed saga.start.closed/Fault and serialization saga.serialization.failed/Fault; this fact does not decide malformed-result policy.

# SAGA-009 — Outgoing metadata, timers and terminal effect ordering

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** Send logical ID becomes EventId; explicit metadata wins, otherwise trigger correlation and trigger MessageId supply defaults. Timer ID is a global ScheduleId; group is SagaId, target saga. Effect order is append order followed by terminal timer-group cancellation.

**Evidence:** SagaEffectWriter.cs; SagaResult.cs; SagaRuntime.cs; SagaTrigger.cs; ScheduleWriter.cs; OutboxEntryFactory.cs.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-009-AC-01 — Send identity and explicit metadata

**Given** handler sends ID X with explicit correlation C and causation D.
**When** owner commits outgoing staging.
**Then** outgoing EventId is X, generated MessageId is delivery identity, and C/D win over trigger metadata.

## SAGA-009-AC-02 — Default send uses triggering delivery causation

**Given** trigger has MessageId X, correlation C and a different causation D; Send omits metadata.
**When** outgoing work is captured.
**Then** correlation inherits C and causation inherits X, not D; when values are null the outgoing context fallback still belongs to outbox.

## SAGA-009-AC-03 — Timer description routes by instance

**Given** handler schedules timer X due at offset instant with trigger metadata.
**When** owner commits.
**Then** schedule ID is X, UTC due is equivalent instant, target is saga, group identifies the SagaId, correlation follows trigger and causation is trigger MessageId.

## SAGA-009-AC-04 — Cancel respects timer scope and current trigger

**Given** two saga instances or targets share cancellation-related IDs and one is current triggering timer.
**When** a handler cancels timer IDs.
**Then** only matching saga group/target active timers cancel; cancelling the current triggering timer is a no-op, and unrelated timers remain.

## SAGA-009-AC-05 — Terminal sends are not discarded

**Given** a Complete/Fail result contains sends and schedules.
**When** owner commits effects and terminal outcome.
**Then** sends remain staged, newly scheduled and other nonterminal timers in that saga group are cancelled after appended effects, excluding the current triggering timer.

## SAGA-009-AC-06 — Timer identity collision is not update

**Given** two results/sagas use the same global timer ID or a retained timer already has it.
**When** effects are saved.
**Then** there is no timer upsert; identity conflict can fail save and participating state/effects require owner rollback.

## SAGA-009-AC-07 — Order has observable cancellation consequences

**Given** Continue result schedules then cancels a timer versus cancels then schedules a previously absent timer.
**When** each isolated owner commits.
**Then** schedule-then-cancel leaves it cancelled; cancel-then-schedule can leave the newly staged timer active, subject to identity uniqueness.

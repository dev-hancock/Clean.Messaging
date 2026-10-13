# SAGA-010 — Timer delivery, business failure and exhaustion

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** Timers use the scheduling delivery contract. Schedule GroupId routes to SagaId and schedule Id is both trigger MessageId and TimerId. Any mapped On overload can be instance-directed. Delivery failure is not SagaResult.Fail.

**Evidence:** SagaScheduleDelivery.cs; SagaProcessor.cs; SagaDefinition.cs; SagaTrigger.cs; SchedulingWorker.cs; ScheduleExecutor.cs.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-010-AC-01 — Missing timer group is permanent

**Given** a saga-target schedule lacks GroupId.
**When** timer delivery runs.
**Then** saga.serialization.failed faults timer delivery without a saga state transition.

## SAGA-010-AC-02 — Missing timer instance can retry

**Given** a valid group identifies no saga.
**When** timer delivery occurs.
**Then** saga.not_found is transient; the timer may retry under scheduling policy, without constructing a saga from timer payload.

## SAGA-010-AC-03 — Active timer invokes mapped handler by instance

**Given** active saga and valid retained mapped timer payload.
**When** timer is delivered.
**Then** that instance receives the mapped message without correlation/create routing; timer identity is the triggering delivery identity for resulting effects.

## SAGA-010-AC-04 — Terminal timer completes without reopening

**Given** terminal saga and even obsolete message mapping/contract.
**When** timer arrives.
**Then** the terminal instance is ignored before active payload/state handling and the timer can complete without new workflow effects.

## SAGA-010-AC-05 — Unknown active mapping is a delivery fault

**Given** active saga type/mapped handler or retained payload is invalid.
**When** timer delivery resolves it.
**Then** definition/handler/type mismatch faults as saga.serialization.failed or shared serializer contract error; it is not successful timer consumption.

## SAGA-010-AC-06 — Exhausted timer leaves active business state

**Given** timer fails transiently until scheduling retry exhaustion or permanently before a committed handler outcome.
**When** failure settles.
**Then** timer is failed but saga remains active; no automatic SagaResult.Fail, compensation or timeout escalation is inferred.

## SAGA-010-AC-07 — Timer handler and consumption join owner commit

**Given** timer handler changes state and captures outgoing work.
**When** schedule delivery transaction succeeds or rolls back.
**Then** state/effects and current timer consumption commit together on success, or remain uncommitted on rollback.

# SAGA-010 — Timer delivery, business failure and exhaustion

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Workflow authors need instance-directed timers whose local delivery outcome does not invent business failure or compensation when timer execution cannot succeed.

The public timer/effect model, saga identity/terminal standards and deliberate NotFound/Serialization exception surface establish routing and outcome distinctions. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Active timer delivery MUST target the owning instance and configured message mapping, not create or correlate a different workflow.
- Timer consumption and participating saga changes MUST commit together; exhausted delivery MUST not automatically business-fail or compensate the saga.

## Contract boundaries

Timers use scheduling ownership, budget and retention rather than a separate saga worker. Missing state can be retryable; due time is not proof of exact firing or of winning a concurrent business transition.

## Acceptance criteria

### SAGA-010-AC-01 — Missing timer group is permanent

**Given** a saga-target timer lacks its required owning group.
**When** delivery interprets its routing.
**Then** SagaSerializationException reports saga.serialization.failed/Fault rather than selecting or creating an arbitrary instance.

### SAGA-010-AC-02 — Missing timer instance can retry

**Given** a timer group identifies an absent saga.
**When** timer processing runs.
**Then** SagaNotFoundException reports saga.not_found/Retry; no business state is invented for the missing instance.

### SAGA-010-AC-03 — Active timer invokes mapped handler by instance

**Given** a timer references an active instance and a valid mapped payload.
**When** the timer is processed.
**Then** the configured handler receives that instance’s state without using ordinary correlation or creation; the timer ScheduleId is both trigger MessageId and TimerId for resulting effects.

### SAGA-010-AC-04 — Terminal timer completes without reopening

**Given** a timer references a terminal instance, including obsolete payload/state.
**When** delivery is accepted by the owner.
**Then** the workflow is not reopened and no handler/state reconstruction is required; that obsolete timer can be consumed successfully.

### SAGA-010-AC-05 — Unknown active mapping is a delivery fault

**Given** an active timer’s retained saga type or payload mapping cannot be resolved.
**When** delivery interprets it.
**Then** saga.serialization.failed/Fault or the shared message contract/serialization failure is exposed, not successful timer consumption or a state transition.

### SAGA-010-AC-06 — Exhausted timer leaves active business state

**Given** a timer for an active saga becomes permanently failed or exhausts delivery attempts.
**When** its delivery outcome is retained.
**Then** the timer is failed but the saga remains active; timer exhaustion alone does not imply SagaResult.Fail, timeout escalation, expiry or compensation.

### SAGA-010-AC-07 — Timer handler and consumption join owner commit

**Given** timer consumption, saga transition and effects share the scheduling owner context/transaction.
**When** successful local timer processing commits.
**Then** those participating changes commit together or remain uncommitted on known owner rollback; schedule consumption is not downstream external delivery.

## Open decisions

None.

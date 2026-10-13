# SAGA-001 — Workflow identity and local outgoing consistency

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour

**Contracts and applicability:** Consumer supplies business state/policy; stable effect identity is distinct from delivery identity. Transactional outbox guarantees apply only to the configured participating local transaction, never arbitrary external effects.

**Evidence:** docs/CODING_STANDARDS.md §§7–9: explicit transaction boundaries, stable logical identity, atomic outgoing work where outbox guarantees apply, cancellation classification.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-001-AC-01 — Effect identity survives retry

**Given** one logical saga Send/ timer operation is retried.
**When** its durable effect is captured again.
**Then** the logical effect/timer identity remains stable; a new delivery identity does not redefine it as a new logical operation.

## SAGA-001-AC-02 — Local state and durable intentions are atomic

**Given** saga state, owning entry completion, outbox and timers use one supported configured scoped transaction.
**When** owner commits successfully.
**Then** participating state and durable effects commit together, not a state-only/outgoing-only successful transition.

## SAGA-001-AC-03 — External effects remain outside rollback guarantee

**Given** a handler invokes an external service before local owner commit.
**When** owner rolls back or repeats invocation.
**Then** external application effects are not claimed to have rolled back or happened exactly once; their policy remains the consumer responsibility.

## SAGA-001-AC-04 — Handler cancellation has distinct owner meaning

**Given** handler receives a cancelled processing token and propagates OperationCanceledException.
**When** owner rolls back.
**Then** the candidate consistent-with-standards outcome is no normal business SagaResult.Fail and no committed participating effects; actual owner cancellation must be verified separately from the conflict-lookup exception window.

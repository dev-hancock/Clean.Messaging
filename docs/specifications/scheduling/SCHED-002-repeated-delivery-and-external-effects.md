# SCHED-002 — Repeated delivery and external effects

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour

**Contracts and applicability:** IScheduleDelivery.Target and Dispatch(ScheduledDispatch, CancellationToken). Delivery is at least once, not exactly once. A delivery adapter must tolerate repeats with the same schedule identity.

**Evidence:** IScheduleDelivery XML contract explicitly requires duplicate tolerance; docs/CODING_STANDARDS.md §§8–9 excludes distributed atomicity and requires assuming repeated external delivery.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-002-AC-01 — Same scheduled operation can be delivered again

**Given** an effect succeeded but its local completion did not commit.
**When** recovery dispatches the schedule again.
**Then** the repeated dispatch carries the same schedule identity and does not claim that the external effect was applied exactly once.

## SCHED-002-AC-02 — Participating outbox state commits together

**Given** a schedule and outgoing rows use the configured scoped context with supported integration.
**When** successful delivery commits its transaction.
**Then** schedule completion and outgoing staging are both durable; neither is committed alone.

## SCHED-002-AC-03 — Owner rollback leaves no local successful effect

**Given** delivery stages outgoing work in the same configured transaction.
**When** that owner rolls back before successful commit.
**Then** neither outgoing staging nor schedule completion is durable; any external effect is outside that rollback guarantee.

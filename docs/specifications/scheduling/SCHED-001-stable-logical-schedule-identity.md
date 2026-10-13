# SCHED-001 — Stable logical schedule identity

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour

**Contracts and applicability:** IMessageScheduler.Schedule<T>; ScheduleId; IScheduleDelivery.Dispatch. A retry describes the same logical operation; it is not a replacement operation. This requirement concerns identity, not duplicate-insert success.

**Evidence:** IMessageScheduler XML contract explicitly says retries must reuse ScheduleId; docs/CODING_STANDARDS.md §9 requires stable logical identity across retries.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-001-AC-01 — Retry preserves operation identity

**Given** a caller retrying one logical scheduling operation.
**When** the retry is staged or redelivered.
**Then** the supplied schedule identity remains the original identity rather than a replacement identity.

## SCHED-001-AC-02 — Distinct operations remain distinct

**Given** two distinct scheduled operations.
**When** the caller supplies their identities.
**Then** the operations use distinct schedule identities; shared correlation does not make them one operation.

## SCHED-001-AC-03 — Message delivery preserves logical event identity

**Given** a valid message-target schedule with identity X.
**When** delivery stages outgoing work.
**Then** every staged target has EventId X; the generated delivery MessageId has a different role, and outgoing metadata does not replace EventId.

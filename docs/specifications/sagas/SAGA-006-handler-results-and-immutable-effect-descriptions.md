# SAGA-006 — Handler results and immutable effect descriptions

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** SagaResult.Continue/Complete/Fail, implicit ValueTask conversion; Send(Guid,payload)/Send(SagaMessage), Schedule(Guid,due,payload)/Schedule(SagaSchedule), Cancel(Guid). SagaMessage exposes ID/payload/optional correlation/causation; SagaSchedule exposes ID/due/payload; SagaCancel exposes TimerId. The public effect hierarchy is not an external apply-plugin port.

**Evidence:** ISagaHandler.cs; SagaResult.cs; SagaEffect.cs; SagaAttempt.cs; SagaRuntime.cs.

**Intent gaps:** [GAP-WF-008](../GAPS.md#gap-wf-008).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-006-AC-01 — Continue complete and fail transition outcomes

**Given** active state and a handler result Continue, Complete or valid Fail.
**When** the owner commits the transition.
**Then** Continue remains active, Complete sets completion and clears diagnostics, Fail sets failure/code/message; terminal outcome does not wait for external delivery.

## SAGA-006-AC-02 — Effect append preserves original result

**Given** a reusable result and added send/schedule/cancel descriptions.
**When** the original and augmented results are used separately.
**Then** the original produces only its original intentions; the augmented one adds effects in append order without mutating the original description.

## SAGA-006-AC-03 — Effect overloads describe equivalent operations

**Given** equivalent values supplied through primitive and object Send/Schedule overloads.
**When** the returned result is processed.
**Then** both forms capture the same logical ID/payload/due/explicit metadata rather than differing durable obligations.

## SAGA-006-AC-04 — Empty IDs and null effects reject

**Given** Guid.Empty Send/Schedule/Cancel, null payload or null message/schedule object.
**When** the corresponding public result API is used.
**Then** invalid argument is rejected before a valid effect description is appended; past due alone is permitted.

## SAGA-006-AC-05 — Null state factory and null handler result fail

**Given** a start factory returns null or a handler returns null.
**When** handling is attempted.
**Then** the invalid result/state is rejected and no successful owner transition/effects commit.

## SAGA-006-AC-06 — Throwing handler does not apply a returned result

**Given** handler mutates its deserialized state and throws or observes cancellation.
**When** the enclosing owner rolls back.
**Then** no mutated saga state or result effects become durable; ordinary exception and cancellation follow their distinct owner classifications.

## SAGA-006-AC-07 — Public cancellation descriptor is not direct effect execution

**Given** new SagaCancel(Guid.Empty) or a custom effect intent is constructed externally.
**When** the supported result APIs are considered.
**Then** constructor alone does not reject empty SagaCancel, but supported Result.Cancel rejects it; no public append/apply port makes arbitrary SagaEffect subclass execution a supported plugin contract.

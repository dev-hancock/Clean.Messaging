# SAGA-014 — Failures, cancellation and malformed result intent

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Unknown behaviour

**Contracts and applicability:** Public SagaNotFoundException is Retry; SagaConflictException and SagaClosedException are Fault and expose SagaType/Key; SagaSerializationException is Fault with optional inner exception. Observed malformed-result discrepancy: SagaFailure validates diagnostics but SagaResult.Fail does not use it. Conflict lookup catches cancellation and preserves original save error.

**Evidence:** Exceptions/*.cs; SagaFailure.cs; SagaResult.Fail; SagaEntry.Fail; SagaConflictResolver.Find; gaps GAP-WF-008 and GAP-WF-010.

**Intent gaps:** [GAP-WF-008](../GAPS.md#gap-wf-008), [GAP-WF-010](../GAPS.md#gap-wf-010).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## Question SAGA-014-Q-01 ? Validated diagnostic value versus raw Fail result

**Scenario:** null/blank code/message, code lengths 200/201 and message lengths 500/501; SagaFailure is constructed versus SagaResult.Fail used.
**Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-008, if current disparity is retained, SagaFailure rejects invalid/overlong values while Fail accepts them until provider/state persistence; tests must not impose universal provider rejection?

## Question SAGA-014-Q-02 ? Cancelled conflict lookup preserves save error today

**Scenario:** qualifying DbUpdateException and caller cancellation during fresh winner lookup; async collision resolution runs.
**Decision question:** Is this conditional candidate the intended policy: pending GAP-WF-010, if current fallback is retained, the original DbUpdateException remains instead of cancellation propagation; normative cancellation policy needs a deliberate decision?

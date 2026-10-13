# SAGA-008 — State application and terminal invariants

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** A handler result describes one attempted transition. State is applied only to an active saga at its expected version; state application precedes outcome/effect capture, without independently committing the unit of work.

**Evidence:** SagaRuntime.cs; SagaAttempt.cs; SagaCommit.cs; SagaEntry.ApplyState/Complete/Fail.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-008-AC-01 — Active result applies one candidate state transition

**Given** an active saga and a result computed from its expected version.
**When** the candidate transition is applied and owner commits.
**Then** serialized state, advanced version and UpdatedAt describe the same committed transition; an effect failure still requires owner rollback.

## SAGA-008-AC-02 — Rejected expected version cannot overwrite state

**Given** a candidate was computed for an earlier version than the saga now carries.
**When** that candidate is applied.
**Then** state/outcome/effects from the stale candidate are rejected rather than silently replacing the newer candidate; database concurrency remains a separate commit guard.

## SAGA-008-AC-03 — Terminal state cannot accept an active transition

**Given** a saga is already complete or failed.
**When** a candidate result attempts state application.
**Then** the terminal instance cannot be reopened by that candidate; terminal replay routing remains a no-op before normal state application.

## SAGA-008-AC-04 — Result construction is not state mutation

**Given** Continue/Complete/Fail and appended intentions have been constructed without processing.
**When** stored saga snapshot is read.
**Then** the result description alone has not changed persisted state/outcome/version or staged an external publication.

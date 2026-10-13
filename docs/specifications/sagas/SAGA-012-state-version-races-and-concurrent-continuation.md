# SAGA-012 — State version races and concurrent continuation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** Version increments on applied state; an attempt requires active state and its expected version. EF optimistic concurrency fences commit, not all handler invocation. No saga-specific reload/retry loop or serialized external-effect guarantee is supplied.

**Evidence:** SagaAttempt.cs; SagaCommit.cs; SagaRuntime.cs; SagaEntry.ApplyState; SagaConfiguration.cs; SagaSaveInterceptor.cs.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-012-AC-01 — One applied transition advances version

**Given** active version V and a valid handler result.
**When** owner commits.
**Then** version becomes V+1 with UpdatedAt and result state/outcome; newly started version zero is advanced by its first applied handler transition.

## SAGA-012-AC-02 — Concurrent continuations fence committed state

**Given** two contexts load active version V and produce different outcomes/effects.
**When** they compete to save/commit.
**Then** one conflicting commit is rejected by optimistic concurrency instead of overwriting the winning state/effects; handlers may both have run.

## SAGA-012-AC-03 — Terminal race cannot reopen state

**Given** one attempt completes/fails while a stale continuation still carries expected active version.
**When** stale attempt applies or saves.
**Then** it cannot commit a reopened active workflow over the winning terminal transition; conflict/owner policy determines recovery.

## SAGA-012-AC-04 — Retries belong to processing owner

**Given** a saga version conflict is surfaced during owner dispatch.
**When** owner applies its retry policy later.
**Then** no saga-only immediate reload/reinvoke is assumed; a later eligible attempt observes then-current durable state.

## SAGA-012-AC-05 — Rejected ownership undoes local workflow commit

**Given** handler ran but owning inbox/outbox/schedule completion is rejected.
**When** the owner rolls back.
**Then** participating saga/effect changes are not committed, although a nontransactional handler effect may already exist.

# SAGA-011 — Owner transactions, rollback, savepoints and replay

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** Saga runtime stages state/effects without independently calling save/starting a business transaction. Inbox/local-outbox dispatchers and schedule executor own supported local transactions. EF saga save hook rejects changed sagas in ambient transactions.

**Evidence:** SagaRuntime.cs; SagaAttempt.cs; SagaSaveInterceptor.cs; InboxDispatcher.cs; LocalOutboxDelivery.cs; ScheduleExecutor.cs; OutboxTransactionInterceptor.cs.

**Intent gaps:** [GAP-WF-006](../GAPS.md#gap-wf-006).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-011-AC-01 — No implicit commit from result application

**Given** valid handling stages saga/outgoing/timer state.
**When** the caller has not saved or committed.
**Then** separate committed readers do not see a completed workflow transition; result construction/handling is not transaction commit.

## SAGA-011-AC-02 — Inbox and local-outbox owners capture all local state

**Given** handler changes saga, business entity, outgoing work and timers in configured owner scope.
**When** entry dispatch commits successfully.
**Then** participating business/saga/effects and owned entry completion are durable together; another context/network effect has no such guarantee.

## SAGA-011-AC-03 — Failed effect rolls back earlier applied local changes

**Given** result state and earlier send effect were staged but a later timer/effect/save fails.
**When** owner rolls back.
**Then** none of the attempt local state or effects is durably committed merely because an earlier effect was staged.

## SAGA-011-AC-04 — Execution strategy may repeat handler

**Given** a local transactional invocation ran but its strategy repeats before a known committed completion.
**When** recovery executes the owner.
**Then** handler invocation can repeat; only committed owner fencing/state/effects determine successful local completion.

## SAGA-011-AC-05 — Ambient changed-saga save is unsupported

**Given** a changed saga participates in ambient TransactionScope.
**When** sync or async SaveChanges begins.
**Then** the saga integration rejects unsupported ambient ownership; unrelated no-saga saves are not rejected by this hook.

## SAGA-011-AC-06 — Outbox composition rejects caller savepoint operations

**Given** saga/schedule/outbox share configured transaction integration.
**When** caller creates or rolls back to a savepoint outside active save.
**Then** outbox integration rejects that unsupported operation; EF internal savepoints during saves are distinct and permitted.

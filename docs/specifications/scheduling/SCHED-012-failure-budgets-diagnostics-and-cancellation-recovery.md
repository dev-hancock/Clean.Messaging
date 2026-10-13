# SCHED-012 — Failure budgets, diagnostics and cancellation recovery

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** Transient/ordinary failure retries until claimed Attempts reaches MaxAttempts; permanent MessageException faults immediately. Scheduling retry is deterministic capped exponential, distinct from shared jittered entry retry.

**Evidence:** SchedulingWorker.cs; ScheduleFailure.cs; ScheduleStore.Retry/Fail; ScheduleEntry.Complete; MessageException.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-012-AC-01 — Permanent failure settles immediately

**Given** a current claimed active schedule fails with FailureAction.Fault MessageException.
**When** failure settlement succeeds.
**Then** FailedAt is recorded immediately with that code/message and claim/lease/retry cleared, even below MaxAttempts.

## SCHED-012-AC-02 — Ordinary and transient failure retry within budget

**Given** a current attempt below MaxAttempts fails ordinarily or with retry-classified MessageException.
**When** settlement succeeds.
**Then** the row remains nonterminal with NextAttemptAt, no claim/lease, and failure diagnostics; it cannot retry before that instant.

## SCHED-012-AC-03 — Exhaustion occurs on failed attempt at boundary

**Given** Attempts equals MaxAttempts or is greater and delivery fails transiently.
**When** settlement succeeds.
**Then** the schedule is failed rather than rescheduled; prior lease-loss claims count toward this decision.

## SCHED-012-AC-04 — Capped exponential delays have no jitter

**Given** fixed clock and attempts 1, 2 and a sufficiently high attempt with valid delays.
**When** retry time is calculated.
**Then** delays equal min(MaxDelay, Delay * 2^max(0, attempt-1)); zero delay is immediate and no random variation is introduced.

## SCHED-012-AC-05 — Diagnostic bounds and fallback code

**Given** MessageException or ordinary exception with long code/message/type.
**When** retry/failure diagnostics persist.
**Then** MessageException.Code or scheduling.unhandled is stored, code/message/exception type are limited to 200/500/500 characters, and ordinary exception text is not a guaranteed sanitized public message.

## SCHED-012-AC-06 — Success retains earlier retry history

**Given** a schedule with failed-attempt diagnostics later delivers successfully.
**When** completion commits.
**Then** DispatchedAt is present and ownership/retry cleared, but Attempts and previous diagnostic values are retained.

## SCHED-012-AC-07 — Shutdown cancellation is not normal message failure

**Given** processing token is cancelled during dispatch.
**When** cancellation propagates.
**Then** the cancellation is not converted into a normal scheduling retry/failure; unfinished persisted work relies on lease/recovery unless already committed.

## SCHED-012-AC-08 — Failed settlement is recoverable unfinished work

**Given** dispatch failed but database retry/failure settlement fails or is cancelled.
**When** the worker later recovers with storage restored.
**Then** no terminal/retry state is inferred from a log; still-active work becomes claimable through expiry/recovery.

# RETRY-003 — Failure-retry budget

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** RetryPolicy permits nonpermanent exceptions only when attempt < MaxAttempts; RetryResult describes a decision.

Pre: valid policy and claimed attempt >=1. Post: permanent faults stop immediately; transient/unknown failures consume the same configured failure budget.

## RETRY-003-AC-01 — Permanent fault early
**Given** attempt 1 with remaining budget and MessageException Fault.
**When** policy evaluates.
**Then** ShouldRetry is false and Delay is zero.

## RETRY-003-AC-02 — Transient or unknown before budget
**Given** Retry or ordinary exception at attempt below MaxAttempts.
**When** policy evaluates.
**Then** ShouldRetry is true with the configured calculated delay.

## RETRY-003-AC-03 — Exhaustion boundary
**Given** a retryable exception at attempt equal to or above MaxAttempts.
**When** policy evaluates.
**Then** ShouldRetry is false without relabelling its original failure category.

## RETRY-003-AC-04 — Decision not execution
**Given** a failure is evaluated.
**When** policy returns RetryResult.
**Then** no consumer invocation or persistence retry is performed by evaluating the decision alone.

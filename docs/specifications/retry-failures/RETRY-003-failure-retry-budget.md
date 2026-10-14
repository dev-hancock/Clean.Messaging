# RETRY-003 — Failure-retry budget

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Processing policy needs a finite failure-retry decision that stops permanent faults while preserving the reason work failed. A decision must not itself execute or settle work.

## Guarantees

- A permanent MessageException fault MUST decline retry immediately with zero delay even when budget remains.
- For a valid supplied attempt below MaxAttempts, transient or unknown processing failure MUST be eligible for retry with the configured calculated delay.
- At or above MaxAttempts, evaluation MUST decline retry without changing the original failure category.
- Evaluation MUST return a decision only; it MUST NOT invoke a consumer or perform persistence retry.

## Contract boundaries

Use valid options and a positive supplied attempt count. This is the public failure-policy budget, not an absolute count of handler invocations, publications, provider transaction re-executions or abandoned claims. What advances the count and whether exhaustion gates acquisition remains [GAP-CLAIM-001](../GAPS.md#gap-claim-001). Shutdown/claim-loss cancellation classification belongs to RETRY-005, not automatic evaluation as an ordinary fault.

## Acceptance criteria

### RETRY-003-AC-01 — Permanent fault early

- **Given** attempt 1 with remaining budget and MessageException Fault.
- **When** policy evaluates.
- **Then** ShouldRetry is false and Delay is zero.

### RETRY-003-AC-02 — Transient or unknown before budget

- **Given** Retry or ordinary exception at attempt below MaxAttempts.
- **When** policy evaluates.
- **Then** ShouldRetry is true with the configured calculated delay.

### RETRY-003-AC-03 — Exhaustion boundary

- **Given** a retryable processing failure has a supplied positive attempt equal to or above MaxAttempts.
- **When** the failure policy evaluates that attempt.
- **Then** ShouldRetry is false while the original transient/unknown failure category is unchanged.

### RETRY-003-AC-04 — Decision not execution

- **Given** a failure is evaluated.
- **When** policy returns RetryResult.
- **Then** no consumer invocation or persistence retry is performed by evaluating the decision alone.

## Open decisions

[GAP-CLAIM-001](../GAPS.md#gap-claim-001) covers acquisition accounting/exhaustion. [GAP-CORE-006](../GAPS.md#gap-core-006) covers out-of-domain direct Evaluate inputs.

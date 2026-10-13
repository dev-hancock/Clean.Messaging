# RETRY-004 — Backoff and jitter boundaries

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** RetryPolicy formulas are fixed/linear/exponential capped before full jitter; see reference/processing.md defaults.

Pre: valid mode and nonnegative delay/cap. Post: delays respect configured mode/cap; jitter is a range, not deterministic timing of eventual handling.

## RETRY-004-AC-01 — Mode delays
**Given** initial 2s, cap 1m, jitter disabled and attempts 1/2/3.
**When** Fixed, Linear and Exponential are evaluated.
**Then** delays are respectively (2,2,2), (2,4,6) and (2,4,8) seconds.

## RETRY-004-AC-02 — Capped delay
**Given** a mode computes delay greater than MaxDelay.
**When** policy evaluates with jitter disabled.
**Then** the result delay equals MaxDelay, not the uncapped delay.

## RETRY-004-AC-03 — Zero delay
**Given** InitialDelay is zero and options otherwise valid.
**When** retryable failure is evaluated.
**Then** zero delay is allowed and no positive minimum is invented.

## RETRY-004-AC-04 — Full jitter
**Given** a positive unjittered bound D.
**When** retries are evaluated with jitter.
**Then** every returned delay is nonnegative and no greater than D; no fixed minimum or distribution is asserted.

## RETRY-004-AC-05 — Retry options validity
**Given** MaxAttempts <=0, negative InitialDelay or MaxDelay < InitialDelay.
**When** standard option validation runs.
**Then** those values are rejected; zero InitialDelay is accepted.

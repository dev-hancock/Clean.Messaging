# RETRY-004 — Backoff and jitter boundaries

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Hosts need selectable retry pacing with bounded delays so temporary failures do not impose uncontrolled waits. Jitter must vary the delay without exceeding the configured bound.

## Guarantees

- For supported Fixed, Linear and Exponential modes, retry delay MUST follow the mode progression and be capped at MaxDelay; AC-01 defines the representative progression.
- Zero InitialDelay MUST permit zero delay without an invented positive minimum.
- With full jitter, every returned delay MUST be nonnegative and no greater than the corresponding unjittered bound; no minimum or distribution is guaranteed.
- Standard options validation MUST reject nonpositive MaxAttempts, negative InitialDelay and MaxDelay below InitialDelay while accepting otherwise valid zero InitialDelay.

## Contract boundaries

Options are valid, configured before use, and attempt count is positive. A calculated delay is not a promise of execution at that instant. The budget/count meaning is RETRY-003 and GAP-CLAIM-001. Undefined modes and live option mutation are separate decisions; this contract does not unify broker/provider/scheduling retries.

## Acceptance criteria

### RETRY-004-AC-01 — Mode delays

- **Given** initial 2s, cap 1m, jitter disabled and attempts 1/2/3.
- **When** Fixed, Linear and Exponential are evaluated.
- **Then** delays are respectively (2,2,2), (2,4,6) and (2,4,8) seconds.

### RETRY-004-AC-02 — Capped delay

- **Given** a mode computes delay greater than MaxDelay.
- **When** policy evaluates with jitter disabled.
- **Then** the result delay equals MaxDelay, not the uncapped delay.

### RETRY-004-AC-03 — Zero delay

- **Given** InitialDelay is zero and options otherwise valid.
- **When** retryable failure is evaluated.
- **Then** zero delay is allowed and no positive minimum is invented.

### RETRY-004-AC-04 — Full jitter

- **Given** a positive unjittered bound D.
- **When** retries are evaluated with jitter.
- **Then** every returned delay is nonnegative and no greater than D; no fixed minimum or distribution is asserted.

### RETRY-004-AC-05 — Retry options validity

- **Given** MaxAttempts <=0, negative InitialDelay or MaxDelay < InitialDelay.
- **When** standard option validation runs.
- **Then** those values are rejected; zero InitialDelay is accepted.

## Open decisions

[GAP-CORE-003](../GAPS.md#gap-core-003) covers undefined modes; [GAP-CORE-009](../GAPS.md#gap-core-009) covers live reconfiguration.

# Retry policy and failure classification — Behavioural specification

**Scope:** `MessageException` (Code, Action, IsTransient, inner exception), `FailureAction`, `MessageFailureFactory.Create`, `MessageFailure`, `FailureCategory`, `RetryOptions`, `RetryBackoff`, `RetryPolicy.Evaluate`, `RetryResult.Fault/After`.

## Purpose
Expose classified failure decisions and bounded failure-retry delays without conflating broker redelivery, provider retries, claims or business workflow failure.

## Contracts, preconditions and postconditions
Pre: positive policy MaxAttempts, valid mode and consistent attempt accounting; runtime supplies claimed attempt count. Post: policy returns retry eligibility and delay or Fault; it does not execute the consumer itself. Failure snapshots are diagnostics, not proof of terminal status.

## Invariants
- Permanent message failures do not become successful outcomes.
- Shutdown/lease loss is not silently converted into an ordinary durable message fault.
- Exhaustion and failure category remain different concepts.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are unresolved decisions, not executable requirements.

## Limitations
- No fixed total number of consumer invocations/external effects, retry-until-success guarantee or schedule/MQTT/EF retry unification.
- Public direct policy inputs outside normal positive attempts are unresolved, and enum validation is incomplete.

## Implementation trace
These locations explain contract ownership; they do not mandate internal implementation structure.

- [src/Clean.Messaging/Exceptions/MessageException.cs](../../../src/Clean.Messaging/Exceptions/MessageException.cs)
- [src/Clean.Messaging/Failures/MessageFailure.cs](../../../src/Clean.Messaging/Failures/MessageFailure.cs)
- [src/Clean.Messaging/Retry/RetryOptions.cs](../../../src/Clean.Messaging/Retry/RetryOptions.cs)
- [src/Clean.Messaging/Retry/RetryOptionsValidator.cs](../../../src/Clean.Messaging/Retry/RetryOptionsValidator.cs)
- [src/Clean.Messaging/Retry/RetryPolicy.cs](../../../src/Clean.Messaging/Retry/RetryPolicy.cs)

## Open decisions
The [gap register](../GAPS.md) records unresolved input domains, provider evidence and proposed changes. [INDEX.md](../INDEX.md) explains classification authority and cross-capability ownership.

## Specifications

- [RETRY-001 — Failure action and safe diagnostics](RETRY-001-failure-action-and-safe-diagnostics.md)
- [RETRY-002 — Failure snapshot values and bounds](RETRY-002-failure-snapshot-values-and-bounds.md)
- [RETRY-003 — Failure-retry budget](RETRY-003-failure-retry-budget.md)
- [RETRY-004 — Backoff and jitter boundaries](RETRY-004-backoff-and-jitter-boundaries.md)
- [RETRY-005 — Processing settlement boundary](RETRY-005-processing-settlement-boundary.md)
- [RETRY-006 — Out-of-domain public policy inputs](RETRY-006-out-of-domain-public-policy-inputs.md)

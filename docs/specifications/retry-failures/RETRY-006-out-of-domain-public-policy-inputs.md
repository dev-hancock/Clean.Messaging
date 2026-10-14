# RETRY-006 — Out-of-domain public policy inputs

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Direct policy callers need a defined invalid-input contract rather than assuming incidental null/count/enum behaviour is a supported retry policy.

## Guarantees

- Valid positive-count failure decisions MUST retain [RETRY-003](RETRY-003-failure-retry-budget.md) and [RETRY-004](RETRY-004-backoff-and-jitter-boundaries.md); no invalid-input outcome is approved here.

## Contract boundaries

Normal processing supplies a nonnull exception, positive attempt and valid configured mode. Zero/negative attempts, null exceptions and undefined modes require the decisions below. Mutable public options are not themselves a live-reconfiguration protocol.

## Acceptance criteria

No additional acceptance outcomes are assigned while the policy below remains unresolved.

## Open decisions

[GAP-CORE-006](../GAPS.md#gap-core-006): decide direct null/count input outcomes. [GAP-CORE-003](../GAPS.md#gap-core-003): decide startup enum rejection. Recommendations remain unapproved: make the supported domain explicit and reject unusable inputs rather than freezing incidental arithmetic.

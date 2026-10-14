# Retry policy and failure classification

[Specification index](../INDEX.md) · [Capability reference](../../reference/processing.md)

## Purpose

Processors and operators need safe classified failure information and bounded failure-retry decisions while preserving cancellation, ownership loss and settlement uncertainty.

## Capability boundaries

MessageException/MessageFailure describe failure, RetryPolicy returns eligibility/delay, and processors conditionally settle durable work. Decision evaluation is not execution. Broker redelivery, provider transaction retries, claims, schedules and business compensation have separate boundaries.

## Decisions and limitations

A supplied attempt count is preserved, but acquisition accounting/exhaustion remains [GAP-CLAIM-001](../GAPS.md#gap-claim-001). No fixed total invocation/effect count is promised. Undefined modes, direct invalid inputs and live option mutation remain [open decisions](../GAPS.md).

## Specifications

- [RETRY-001 — Failure action and safe diagnostics](RETRY-001-failure-action-and-safe-diagnostics.md)
- [RETRY-002 — Failure snapshot values and bounds](RETRY-002-failure-snapshot-values-and-bounds.md)
- [RETRY-003 — Failure-retry budget](RETRY-003-failure-retry-budget.md)
- [RETRY-004 — Backoff and jitter boundaries](RETRY-004-backoff-and-jitter-boundaries.md)
- [RETRY-005 — Processing settlement boundary](RETRY-005-processing-settlement-boundary.md)
- [RETRY-006 — Out-of-domain public policy inputs](RETRY-006-out-of-domain-public-policy-inputs.md)

# INBOX-013 — Replay failure transparency requirement

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

An operator must not be told that replay succeeded merely because a failed persistence operation was converted into a small or zero result count.

## Guarantees

- An unreconciled replay persistence failure MUST propagate as failure rather than returning a normal replay result.
- Reconciliation MUST establish the relevant intended durable outcome before permitting a successful return; missing additions alone MUST NOT be treated as proof that the failed operation succeeded.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards section 7 establishes failure transparency and prohibits swallowing failures as success; stable replay result truth is independent of a particular reconciliation algorithm.

## Contract boundaries

A concurrently completed duplicate can provide outcome evidence, but a generic persistence exception is not duplicate evidence by itself. This requirement does not choose the precise duplicate classification, count attribution or cursor protocol; those remain policy decisions. A thrown error can still leave partial or uncertain durable effects.

## Acceptance criteria

### INBOX-013-AC-01 — Do not turn unreconciled replay failure into success

- **Given** a replay save fails for a nonduplicate reason and the intended durable outcome has not been confirmed.
- **When** replay reconciles or reports the operation.
- **Then** the failure remains observable rather than a normal Matched result with zero or fewer Replayed entries.

## Open decisions

[GAP-DUR-003](../GAPS.md#gap-dur-003) leaves reconciliation, page progress and counts unresolved. Their implementation choices cannot authorize concealing failure.

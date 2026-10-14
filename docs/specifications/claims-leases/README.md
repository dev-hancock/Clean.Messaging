# Claims, leases and settlement ownership

[Specification index](../INDEX.md) · [Capability reference](../../reference/processing.md)

## Purpose

Workers acquire temporary durable target ownership so abandoned work can be recovered without obsolete owners overwriting a successor. This catalogue separates acquisition, settlement, renewal and direct-input policy.

## Capability boundaries

IDurableStore/OwnedEntry describe ownership of inbox/outbox targets; ClaimRenewal signals cooperative loss. Scheduling has its [own ownership contract](../scheduling/README.md). A lease fences durable state, not external effects or uncooperative handlers. Logical identity is independent of ownership.

## Decisions and limitations

The host supplies valid operational settings and a consistent clock basis. Acquisition accounting, exact deadline equality, unusual renewal inputs and provider certification remain open; see the [decision register](../GAPS.md).

## Specifications

- [CLAIM-001 — Recoverable ownership of durable work](CLAIM-001-claim-eligibility-and-attempt-accounting.md)
- [CLAIM-002 — Stale settlement rejection](CLAIM-002-stale-settlement-rejection.md)
- [CLAIM-003 — Renewal and settlement preserve ownership lifecycle](CLAIM-003-owned-state-and-expiry-predicates.md)
- [CLAIM-004 — Renewal loss and cooperative cancellation](CLAIM-004-renewal-loss-and-cooperative-cancellation.md)
- [CLAIM-005 — Lease arithmetic and caller invariants](CLAIM-005-lease-arithmetic-and-caller-invariants.md)

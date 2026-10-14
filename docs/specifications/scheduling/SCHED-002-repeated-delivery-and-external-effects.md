# SCHED-002 — Repeated delivery and external effects

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Delivery implementers and host composers need recoverable scheduled delivery without mistaking local consumption for an exactly-once external effect.

IScheduleDelivery explicitly specifies at-least-once delivery; the local atomicity and external-effects standards establish the transaction boundary. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Delivery implementations MUST tolerate repeat delivery of the same scheduled operation.
- Participating outgoing state and schedule completion MUST commit together or leave no confirmed local successful effect after rollback.

## Contract boundaries

Atomicity requires one scoped context and owning transaction. Direct external I/O, other contexts and broker receipt are excluded; an uncertain commit is not confirmed rollback.

## Acceptance criteria

### SCHED-002-AC-01 — Same scheduled operation can be delivered again

**Given** one retained operation whose earlier attempt was rolled back or has an uncertain outcome.
**When** recovery delivers it again.
**Then** the operation identity is unchanged and the delivery implementation tolerates the repetition; external effects are not promised to occur once.

### SCHED-002-AC-02 — Participating outbox state commits together

**Given** schedule completion and outgoing rows participating in one context/transaction.
**When** successful scheduled delivery commits.
**Then** both completion and the outgoing intentions are durable in that local commit, not independently committed halves.

### SCHED-002-AC-03 — Owner rollback leaves no local successful effect

**Given** local delivery writes participating in an owner transaction that is known to roll back.
**When** the failed attempt is inspected.
**Then** neither successful schedule completion nor those outgoing rows is committed by that attempt; external effects are outside this result.

## Open decisions

None.

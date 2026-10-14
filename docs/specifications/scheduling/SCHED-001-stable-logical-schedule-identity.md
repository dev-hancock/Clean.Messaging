# SCHED-001 — Stable logical schedule identity

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Callers scheduling deferred work need one logical operation identity across retries so that recovery cannot accidentally describe a new timer.

The public IMessageScheduler XML retry obligation and the coding standards on logical versus delivery identity establish this contract. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Retries MUST preserve the ScheduleId of the same logical operation; distinct operations MUST use distinct identities.
- Message-target delivery MUST preserve that identity as outgoing EventId, not substitute a transport MessageId.

## Contract boundaries

Stable identity is not an equal-insert success result, an upsert or indefinite deduplication. Record retention bounds protection; downstream transports own delivery IDs.

## Acceptance criteria

### SCHED-001-AC-01 — Retry preserves operation identity

**Given** one logical scheduling operation is being retried.
**When** the caller supplies the retry description.
**Then** the ScheduleId is the original operation identity, not a newly allocated retry identity.

### SCHED-001-AC-02 — Distinct operations remain distinct

**Given** two independently intended scheduled operations.
**When** the caller identifies them.
**Then** their ScheduleIds are distinct, even when their payloads or due times coincide.

### SCHED-001-AC-03 — Message delivery preserves logical event identity

**Given** a committed message-target schedule identified by S.
**When** its outgoing delivery is durably staged.
**Then** the outgoing EventId is S; its delivery MessageId remains a separate identity.

## Open decisions

None.

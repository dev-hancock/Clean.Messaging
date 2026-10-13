# OUTBOX-001 — Enqueue stages a concrete envelope and target snapshot

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: IOutbox/Outbox.Enqueue, OutboxMessage<T>, OutboxEntryFactory.Create, OutboxBatch and OutboxEntry. [Outbox](../../reference/outbox.md), [contracts](../../reference/contracts.md) and [transport](../../reference/transport.md) are descriptive, not normative.

Purpose: stage deferred local/external work in the owning persistence context. Contracts: synchronous Enqueue<TMessage> has no cancellation/save/commit promise; factory Create<T> returns batch; OutboxMessage Id/Message/CorrelationId/CausationId. Preconditions: nonnull envelope/payload, nonempty Id, registered concrete contract and usable providers. Postcondition: targets have a fresh delivery MessageId shared across the batch; caller Id becomes logical EventId. Factory serializes concrete runtime type and evaluates providers against the original object before staging. Destination/transport/target snapshots and UTC CreatedAt are persisted per target.

## OUTBOX-001-AC-01 — Do not confuse staging with persistence or delivery

- **Given** a valid event and an owning unsaved context.
- **When** Enqueue returns and the context is later abandoned.
- **Then** no successful save, durable rows, consumer completion or external publication is implied.

## OUTBOX-001-AC-02 — Preserve logical identity with new delivery identity

- **Given** a valid caller event Id and several targets.
- **When** Create/Enqueue creates a batch and the owning save commits.
- **Then** all targets share fresh MessageId, caller Id as EventId, serialized concrete contract/payload and one creation time; another factory invocation can generate a different MessageId.

## OUTBOX-001-AC-03 — Reject invalid envelope and serialization

- **Given** null envelope/payload, empty Id, missing concrete contract or serializer failure.
- **When** Create/Enqueue runs.
- **Then** it fails without a successfully staged batch; record construction alone supplies no runtime validation.

## OUTBOX-001-AC-04 — Snapshot targets before storage

- **Given** target providers calculate transport/destination from the original concrete payload.
- **When** factory creation succeeds, routing logic later changes and the batch is saved.
- **Then** stored target/transport/destination remain the staged values; providers see the concrete runtime type and original payload.

# SAGA-007 — State serialization and retained schema compatibility

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** SagaOptions.SerializerOptions independently configure state JSON. Durable message payload serialization uses registered message contracts; numeric Version is optimistic concurrency, not JSON schema version.

**Evidence:** SagaStateSerializer.cs; SagaAttempt.cs; SagaRuntime.cs; SagaEntry.cs; shared IMessageSerializer.

**Intent gaps:** [GAP-WF-015](../GAPS.md#gap-wf-015).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-007-AC-01 — State mutations round-trip using state options

**Given** active valid state and configured JSON options.
**When** handler mutates state and owner commits then later continues.
**Then** the persisted JSON can reconstruct that configured state and its committed mutations; outgoing serializer options are not implicitly the state options.

## SAGA-007-AC-02 — Malformed null or unsupported state is permanent

**Given** retained active state JSON is malformed/null or state serialization is unsupported.
**When** state deserialize/serialize is attempted.
**Then** saga.serialization.failed is permanent, with no successful owner state/effects transition.

## SAGA-007-AC-03 — Retained type and message compatibility are required

**Given** retained active saga type/JSON or timer contract no longer resolves compatibly.
**When** execution is attempted.
**Then** the corresponding definition/state/message error fails rather than automatically migrating, upcasting or inventing a replacement state.

## SAGA-007-AC-04 — Terminal replay does not need purged state

**Given** terminal saga state is the tombstone JSON null.
**When** same-start replay, terminal continuation or terminal timer is handled.
**Then** the terminal no-op path does not reopen or require reconstructing purged state.

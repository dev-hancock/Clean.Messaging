# SAGA-007 — State serialization and retained schema compatibility

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Workflow hosts need retained state to remain interpretable across recovery and deployment without confusing state concurrency with schema evolution.

The state/message compatibility and durable contract standards, SagaOptions state serializer and public SagaSerializationException establish serialization and rejection boundaries. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Successful state changes MUST round-trip through the configured state serializer, separately from message payload configuration.
- Uninterpretable active state MUST surface a permanent serialization failure rather than a successful transition; terminal replay MUST not require purged state.

## Contract boundaries

Host state/contract compatibility is required for retained work. No automatic migration is supplied; selecting supported schema evolution remains GAP-WF-015. Concurrency Version is not a schema version.

## Acceptance criteria

### SAGA-007-AC-01 — State mutations round-trip using state options

**Given** an active workflow uses configured state JSON options and a handler successfully mutates state.
**When** the owner commits and later processing reads the state.
**Then** the mutation round-trips through those state options, independently of the message serializer’s options.

### SAGA-007-AC-02 — Malformed null or unsupported state is permanent

**Given** active state is malformed, deserializes null or is unsupported by its configured serializer.
**When** the runtime interprets or serializes it.
**Then** SagaSerializationException exposes saga.serialization.failed/Fault rather than a successful transition.

### SAGA-007-AC-03 — Retained type and message compatibility are required

**Given** a deployment must process retained saga types, state and message contracts.
**When** retained active work is interpreted.
**Then** an incompatible retained definition, state or message exposes its corresponding interpretation error rather than automatically migrating, upcasting or inventing replacement state; compatible schemas remain host-owned and numeric Version is not a migration mechanism.

### SAGA-007-AC-04 — Terminal replay does not need purged state

**Given** a terminal workflow’s payload state has been purged.
**When** same-start replay or terminal continuation arrives.
**Then** terminal replay/no-op behavior does not reconstruct the purged state or reopen the workflow.

## Open decisions

Separate boundary questions remain unapproved in the [decision register](../GAPS.md): `GAP-WF-015`. These do not suspend the independently justified guarantees above.

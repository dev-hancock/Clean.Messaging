# MQTT-004 — Incoming metadata, serialization and contract compatibility

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

MQTT peers need application identities and declared payload contracts independent of packet IDs, QoS and duplicate/retain flags.

Authority: Deliberate shared producer/consumer wire metadata and registered-contract compatibility, protected by coding standards on reserved metadata, identity and validation. Repeated-property precedence and metadata loss lack independent approval. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Successful admission MUST preserve valid message/event/context IDs under the exact-case reserved metadata contract. Broker flags/packet IDs MUST NOT replace application identity.
- Absent contract declarations MUST use the registered route payload contract; absent event identity MUST use the durable delivery-identity default. Missing registration MUST fail instead of inferring a CLR contract.
- Missing required identity, malformed/empty present IDs and mismatched declarations MUST reject admission as poison without replacement identity.
- Identical payload bytes MUST retain their meaning across segmentation.

## Contract boundaries

Reserved names: message-id, event-id, contract, correlation-id, causation-id. Declarations match route payload contract ordinally, including case. Delivery identity is required; event/context identities are optional but present IDs must be nonempty GUIDs. Header/content-type propagation and serializer authority remain unresolved.

## Acceptance criteria

### MQTT-004-AC-01 — Valid peer metadata admits stable application identities

- **Given** compatible payload with message-id, distinct event-id and valid optional context.
- **When** MQTT admission succeeds.
- **Then** the mapped durable message retains those IDs independently of packet ID, QoS, duplicate and retain flags.

### MQTT-004-AC-02 — Optional declaration and event identity have defined absence semantics

- **Given** valid payload/message-id without contract or event-id.
- **When** admission succeeds or route payload registration is absent.
- **Then** registered route contract controls decoding and event identity defaults to delivery identity; missing registration fails rather than inferring a CLR name.

### MQTT-004-AC-03 — Malformed identity and mismatched contract are poison

- **Given** missing message identity, malformed/empty present reserved ID or case/value-mismatched contract.
- **When** metadata is evaluated.
- **Then** poison prevents successful admission without treating invalid metadata as absent or generating replacement identity.

## Open decisions

The original IDs and exact heading anchors below remain reserved, not normative scenarios. No replacement IDs are assigned before a decision.

### MQTT-004-AC-04 — Last exact-name property wins

Should repeated exact reserved properties be rejected, required to agree or resolved by deliberate precedence? Exact casing does not approve last-value-wins. Decision [GAP-INTENT-TX-003](../GAPS.md#gap-intent-tx-003) remains unapproved.

### MQTT-004-AC-05 — Payload segmentation and Content Type do not alter decoding policy

The original segmentation criterion also fixes serializer selection and metadata loss. Reserve the whole composite, not a segmentation-only reuse of its ID. See [GAP-TX-004](../GAPS.md#gap-tx-004); the recommendation remains unapproved.

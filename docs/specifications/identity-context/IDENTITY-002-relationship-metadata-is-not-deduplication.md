# IDENTITY-002 — Relationship metadata is not deduplication

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Related operations need correlation and causation without being mistaken for the same operation. Follow-on messages must identify the actual causing delivery where the API inherits that relationship.

## Guarantees

- Shared correlation alone MUST NOT suppress distinct event identities during admission or staging.
- When the applicable outgoing API inherits causation, it MUST use the current causing delivery MessageId, not an inferred EventId or correlation identity.

## Contract boundaries

Correlation/causation are optional metadata, not uniqueness keys, tracing infrastructure or workflow state. Explicit values and propagation are owned by the initiating capability API; this does not impose inheritance on every message-producing operation.

## Acceptance criteria

### IDENTITY-002-AC-01 — Shared correlation

- **Given** two distinct operations share one correlation ID but have distinct event identities.
- **When** both are admitted or staged for a target.
- **Then** correlation alone does not suppress either operation.

### IDENTITY-002-AC-02 — Causation distinction

- **Given** an outgoing operation is caused by a current incoming delivery.
- **When** the applicable API inherits causation.
- **Then** causation represents the causing delivery ID, not an inferred event ID or correlation key.

## Open decisions

None.

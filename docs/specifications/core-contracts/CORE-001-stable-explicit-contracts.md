# CORE-001 — Stable explicit contracts

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Producers and consumers need retained messages to remain interpretable across runtime refactoring. A stable named contract separates message meaning from deployment-specific CLR assembly identity.

## Guarantees

- A registered type/name association MUST be resolvable in both directions using the configured durable contract name.
- Reading retained data with an unchanged agreed contract and compatible shape MUST restore the represented typed values; refactoring runtime internals MUST NOT replace the durable identity with an assembly-qualified type name.

## Contract boundaries

The host owns schemas, registration and compatible producer/consumer evolution. This does not supply automatic upcasting, schema negotiation or recovery of removed contracts. JSON serialization details are CORE-004; unknown contracts fail under CORE-003.

## Acceptance criteria

### CORE-001-AC-01 — Registered association

- **Given** a message type registered as `orders.created.v1`.
- **When** its registered contract is requested.
- **Then** the configured name is returned and resolving that name identifies the registered type.

### CORE-001-AC-02 — Retained compatible message

- **Given** a retained message using an unchanged agreed contract and compatible shape.
- **When** the serializer reads it after runtime internals are refactored.
- **Then** the caller receives the agreed typed message with its represented values.

## Open decisions

None.

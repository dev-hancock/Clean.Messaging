# CORE-001 — Stable explicit contracts

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Evidence:** Coding standards §9 and §14 require persisted names and shapes to remain compatibility contracts; public registry and serializer signatures establish lookup/translation operations.

Pre: a registered type/name association and a compatible payload. Post: serializing and reading retained data preserves the agreed message meaning; replacing internals must not replace names with assembly-qualified type identity.

## CORE-001-AC-01 — Registered association
**Given** a message type registered as `orders.created.v1`.
**When** its registered contract is requested.
**Then** the configured name is returned and resolving that name identifies the registered type.

## CORE-001-AC-02 — Retained compatible message
**Given** a retained message using an unchanged agreed contract and compatible shape.
**When** the serializer reads it after runtime internals are refactored.
**Then** the caller receives the agreed typed message with its represented values.

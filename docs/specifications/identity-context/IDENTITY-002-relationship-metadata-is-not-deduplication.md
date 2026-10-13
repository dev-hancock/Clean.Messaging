# IDENTITY-002 — Relationship metadata is not deduplication

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Evidence:** Coding standards §9 separates correlation/causation from logical identities; repository instructions require their preservation.

Pre: two distinct operations may share correlation; optional causation describes a preceding delivery. Post: preserving these values does not coalesce distinct operations.

## IDENTITY-002-AC-01 — Shared correlation
**Given** two distinct operations share one correlation ID but have distinct event identities.
**When** both are admitted or staged for a target.
**Then** correlation alone does not suppress either operation.

## IDENTITY-002-AC-02 — Causation distinction
**Given** an outgoing operation is caused by a current incoming delivery.
**When** the applicable API inherits causation.
**Then** causation represents the causing delivery ID, not an inferred event ID or correlation key.

# CORE-002 — Registry uniqueness and selection

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** Registry construction and lookups; reference/contracts.md is descriptive source evidence, not independent approval.

Pre: registrations supplied before registry resolution. Post: one name per type, one type per name, and globally unique consumer IDs in the supplied registries; lookup uses exact type and ordinal identity. Failures occur at registry resolution rather than durable admission.

## CORE-002-AC-01 — Duplicate contract type or name
**Given** two entries share a CLR type or contract name, including an identical repeated entry.
**When** the contract registry is resolved.
**Then** construction fails rather than silently replacing either association.

## CORE-002-AC-02 — Duplicate consumer identity
**Given** two consumers share an identity even across different message types.
**When** the consumer registry is resolved.
**Then** construction fails rather than choosing one consumer.

## CORE-002-AC-03 — Exact message type
**Given** a consumer registered only for a base message type.
**When** consumers for a derived concrete type are requested.
**Then** lookup faults with `consumer.message_unhandled` rather than inferring base-type fan-out.

## CORE-002-AC-04 — Ordinal names
**Given** a registered contract or consumer identity differs from a requested identity only in case.
**When** that requested identity is resolved.
**Then** it is not treated as the registered identity.

## CORE-002-AC-05 — Complete consumer enumeration
**Given** several distinct consumer registrations exist.
**When** All and exact-type Get are examined.
**Then** All exposes each registration and Get exposes the registrations of that exact type without a promised enumeration order.

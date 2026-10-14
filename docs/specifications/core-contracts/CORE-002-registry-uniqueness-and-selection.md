# CORE-002 — Registry uniqueness and selection

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

A retained contract or target identity must select an unambiguous registered message/consumer. Hosts need to enumerate configured consumers without implicit inheritance routing or case folding changing target meaning.

## Guarantees

- Registry resolution MUST reject duplicate contract types/names, including identical repeated associations, and duplicate consumer IDs across all message types.
- Consumer lookup MUST select registrations for the exact message type; absence MUST report the permanent consumer.message_unhandled failure rather than infer base-type fan-out.
- Contract names and consumer IDs MUST use ordinal, case-sensitive identity; differing case MUST NOT silently select another identity.
- All MUST expose every consumer registration and exact-type Get MUST expose every matching registration, without a guaranteed enumeration order.

## Contract boundaries

Registrations are supplied before registry resolution. Generic consumer contravariance is not registration of a derived type. Duplicate rejection occurs on registry resolution, not necessarily on AddContract/AddConsumer. Invalid direct registration records remain CORE-005. This is identity/selection, not processing order.

## Acceptance criteria

### CORE-002-AC-01 — Duplicate contract type or name

- **Given** two entries share a CLR type or contract name, including an identical repeated entry.
- **When** the contract registry is resolved.
- **Then** construction fails rather than silently replacing either association.

### CORE-002-AC-02 — Duplicate consumer identity

- **Given** two consumers share an identity even across different message types.
- **When** the consumer registry is resolved.
- **Then** construction fails rather than choosing one consumer.

### CORE-002-AC-03 — Exact message type

- **Given** a consumer registered only for a base message type.
- **When** consumers for a derived concrete type are requested.
- **Then** lookup faults with `consumer.message_unhandled` rather than inferring base-type fan-out.

### CORE-002-AC-04 — Ordinal names

- **Given** a registered contract or consumer identity differs from a requested identity only in case.
- **When** that requested identity is resolved.
- **Then** it is not treated as the registered identity.

### CORE-002-AC-05 — Complete consumer enumeration

- **Given** distinct consumers are registered for several message types.
- **When** a caller enumerates All and requests Get for an exact registered type.
- **Then** All contains every registration and Get contains all and only registrations for that type, irrespective of enumeration order.

## Open decisions

[GAP-CORE-001](../GAPS.md#gap-core-001) covers validation bypass through public registration records.

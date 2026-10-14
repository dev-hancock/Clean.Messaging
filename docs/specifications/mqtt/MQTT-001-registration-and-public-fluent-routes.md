# MQTT-001 — Registration and public fluent routes

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Host developers need typed incoming subscriptions and outgoing publications without provider types leaking into consumer contracts.

Authority: Deliberate public On/Map/Subscribe/Publish contracts and modular responsibilities, protected by coding standards on composition and public compatibility. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Completed identity routes MUST preserve payload values and mapped routes MUST apply their configured typed maps.
- Null callbacks and absent completed routes MUST fail configuration rather than establish a silently nonoperational adapter.
- Provider registration alone MUST NOT supply durable persistence, consumer processing or target completion.

## Contract boundaries

Inbox routes terminate with Subscribe; outgoing routes terminate with Publish. On/Map alone are incomplete. Hosts separately compose durable integration, schemas, contracts and consumers.

## Acceptance criteria

### MQTT-001-AC-01 — Identity and mapped routes compose the correct boundary

- **Given** completed identity and typed mapped routes with separately composed durable contracts.
- **When** configured incoming/outgoing types are processed.
- **Then** identity routes preserve payload values and mapped routes apply their configured maps.

### MQTT-001-AC-02 — Incomplete or null registration fails clearly

- **Given** null configuration/route callbacks or callbacks containing only On/Map.
- **When** adapter registration runs.
- **Then** null callbacks fail argument validation and zero completed routes raise InvalidOperationException; null services cannot establish valid registration, with explicit inbox argument rejection.

### MQTT-001-AC-03 — Provider registration alone does not process durable work

- **Given** MQTT services/routes/options without durable integration.
- **When** admission or durable dispatch is requested.
- **Then** registration supplies neither tables, missing stores/consumers/contracts nor independent durable completion.

## Open decisions

None.

# TRANSPORT-001 — Preserve representation and identity boundaries

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Hosts and consumers need a change of representation without a change of logical operation. Delivery, event and context identities must remain meaningful across providers and repeated delivery.

Authority: Coding standards on provider separation, identity roles and public compatibility establish these requirements independently of factories. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Mapping MUST preserve supplied delivery, event, correlation and causation identities in their respective roles. Payload properties and attempt IDs MUST NOT replace envelope identity.
- Remapping retained outgoing work MUST preserve its stored message and event IDs.
- Consumer and provider-neutral mapper contracts MUST remain usable without ORM transaction or broker packet types.

## Contract boundaries

Incoming identities must satisfy durable admission validation; outgoing identities come from retained work. Mapping changes representation, not target ownership or acknowledgement. Stable identity enables duplicate protection but does not establish exactly-once effects.

## Acceptance criteria

### TRANSPORT-001-AC-01 — Distinct delivery and logical identities survive mapping

- **Given** valid incoming or stored outgoing work with distinct delivery and event IDs and explicit context IDs.
- **When** the configured typed mapping boundary is crossed.
- **Then** admission or the resulting envelope retains each supplied identity in its original role.

### TRANSPORT-001-AC-02 — Retry does not create a new logical operation

- **Given** a retained outgoing target with unresolved prior publication.
- **When** the target is remapped with unchanged data and configuration.
- **Then** the envelope retains stored message/event IDs rather than assigning a new operation.

### TRANSPORT-001-AC-03 — Consumer contracts remain provider-neutral

- **Given** a consumer contract and provider-neutral mapper.
- **When** they are implemented without an ORM or MQTT client dependency.
- **Then** their signatures require neither transaction objects nor broker packet types.

## Open decisions

None.

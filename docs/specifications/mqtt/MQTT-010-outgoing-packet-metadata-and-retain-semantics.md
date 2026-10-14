# MQTT-010 — Outgoing packet metadata and retain semantics

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

MQTT peers need stable authoritative identity metadata protected from caller overrides and packet attributes distinct from durable target retention.

Authority: Deliberate shared producer/consumer metadata, packet options and public retain default, protected by coding standards on reserved metadata, identity, serialized compatibility and diagnostics. Optional empty-ID asymmetry is not approved interoperability. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Packets MUST carry stored destination, mapped bytes, configured QoS, route retain and authoritative required message/event IDs plus mapped contract. Required IDs MUST be nonempty D-format GUIDs.
- Exact reserved header names MUST NOT override factory metadata; nonreserved duplicate headers and nonblank content type MUST remain representable.
- Missing/empty required IDs and blank destination MUST fail permanent OutboxDispatchException without successful packet publication.
- Route retain MUST default false and MUST NOT imply durable retention or exactly-once processing.

## Contract boundaries

Reserved exact-case names: message-id, event-id, contract, correlation-id, causation-id. Meaningful optional IDs carry context; case-distinct headers are not reserved aliases. Packet validity does not prove connectivity, broker acceptance or recipient effects. Connected-client acquisition can precede packet validation, so failure does not exclude a connection attempt.

## Acceptance criteria

### MQTT-010-AC-01 — Packet includes stored route destination, payload and application identities

- **Given** valid mapped data, stored topic, route retain and configured QoS.
- **When** packet construction/publication occurs.
- **Then** topic/bytes/QoS/retain match inputs and UTF-8 reserved properties contain nonempty D-format message/event IDs and mapped contract; retain defaults false independently of durable retention.

### MQTT-010-AC-02 — Reserved metadata cannot be overridden by caller headers

- **Given** exact reserved override attempts, duplicate nonreserved headers and nonblank content type.
- **When** the packet is constructed.
- **Then** factory metadata prevails; nonreserved UTF-8 duplicates and content type remain, while case-distinct names are not reserved aliases.

### MQTT-010-AC-03 — Missing required IDs or destination reject outgoing construction

- **Given** missing/empty required message/event ID or blank destination.
- **When** packet construction runs.
- **Then** permanent OutboxDispatchException prevents reported successful publication, without excluding earlier connection acquisition.

## Open decisions

The original IDs and exact heading anchors below remain reserved, not normative scenarios. No replacement IDs are assigned before a decision.

### MQTT-010-AC-04 — Optional empty IDs expose current peer incompatibility

Should empty optional IDs be omitted, rejected before publish or accepted as absent by both peers? Keep the original asymmetric producer/consumer composite reserved. See [GAP-TX-005](../GAPS.md#gap-tx-005); the recommendation remains unapproved.

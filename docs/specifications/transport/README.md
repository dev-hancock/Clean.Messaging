# Transport mapping

Hosts use provider-neutral payload mapping to admit external representations and construct outgoing envelopes without changing identity or confusing mapping with delivery.

## Capability boundaries

Admission, consumer completion, publication and durable target settlement are distinct. Required identities remain stable; external publication can repeat. Metadata propagation, textual contract authority, conflicting identity and synchronous cancellation choices remain explicit in the numbered specifications.

These specifications separate independently supported guarantees from reserved original criteria. Open recommendations remain unapproved; source observations do not settle policy.

## Capability catalogue

- [TRANSPORT-001 — Preserve representation and identity boundaries](TRANSPORT-001-preserve-representation-and-identity-boundaries.md)
- [TRANSPORT-002 — Envelope and payload serialization surface](TRANSPORT-002-envelope-and-payload-serialization-surface.md)
- [TRANSPORT-003 — Registration and exact mapping resolution](TRANSPORT-003-registration-and-exact-mapping-resolution.md)
- [TRANSPORT-004 — Incoming typed admission and metadata loss boundary](TRANSPORT-004-incoming-typed-admission-and-metadata-loss-boundary.md)
- [TRANSPORT-005 — Admission failures, duplicates and cancellation](TRANSPORT-005-admission-failures-duplicates-and-cancellation.md)
- [TRANSPORT-006 — Outgoing payload construction and permanent mapping faults](TRANSPORT-006-outgoing-payload-construction-and-permanent-mapping-faults.md)
- [TRANSPORT-007 — External publication and per-target completion are separate](TRANSPORT-007-external-publication-and-per-target-completion-are-separate.md)
- [TRANSPORT-008 — Unapproved compatibility and synchronous cancellation decisions](TRANSPORT-008-unapproved-compatibility-and-synchronous-cancellation-decisions.md)

## Related contracts and decisions

- [GAP-TX-002](../GAPS.md#gap-tx-002)
- [GAP-TX-003](../GAPS.md#gap-tx-003)
- [GAP-TX-004](../GAPS.md#gap-tx-004)
- [GAP-TX-008](../GAPS.md#gap-tx-008)

- [MQTT](../mqtt/README.md)
- [repository instructions](../../../.github/copilot-instructions.md)
- [coding standards](../../CODING_STANDARDS.md)
- [transport reference](../../reference/transport.md)
- [architecture](../../reference/architecture.md)
- [contracts](../../reference/contracts.md)
- [configuration](../../reference/configuration.md)
- [delivery semantics](../../explanation/delivery-semantics.md)
- [identity](../../explanation/message-identity.md)
- [package contract and source map](../../reference/contract-map.md)
